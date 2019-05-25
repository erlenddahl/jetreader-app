/*global Ebook*/
/*global Messages*/
/*global Base64*/
/*global csCallback*/
/*global Hammer*/
/*global Gestures*/
/*global KeyStrokes*/


function CodeTimer() {
    this.timings = [];
    this.prev = performance.now();
}

CodeTimer.prototype.time = function () {
    var now = performance.now();
    this.timings.push(now - this.prev);
    this.prev = now;
}

CodeTimer.prototype.sendDebug = function (desc) {
    var data = desc + " :: " + JSON.stringify(this.timings);
    window.Messages.send("Debug", data);
    return data;
}

var pageLoadTime = performance.now();

window.Ebook = {
    pageWidth: 0,
    totalPages: 0,
    currentPage: 1,
    fontSize: 0,
    webViewWidth: 0,
    webViewHeight: 0,
    webViewMargin: 0,
    scrollSpeed: 0,
    doubleSwipe: false,
    nightMode: false,
    init: function (width, height, margin, fontSize, scrollSpeed, doubleSwipe, nightMode) {
        this.webViewWidth = width;
        this.webViewHeight = height;
        this.webViewMargin = margin;
        this.fontSize = fontSize;
        this.scrollSpeed = scrollSpeed;
        this.doubleSwipe = doubleSwipe;
        this.nightMode = nightMode;
        this.debugging = true;
        this.commands = [
            {
                height: 3/10,
                cells: [
                    {
                        tap: "prevPage",
                        width: 1 / 3
                    },
                    {
                        tap: "visualizeCommandCells",
                        press: "openQuickSettings",
                        width: 1 / 3,
                        showText: true
                    },
                    {
                        tap: "nextPage",
                        width: 1 / 3
                    }
                ]
            },
            {
                height: 3/10,
                cells: [
                    {
                        tap: "prevPage",
                        width: 1 / 3
                    },
                    {
                        tap: "toggleFullscreen",
                        press: "openQuickSettings",
                        width: 1 / 3,
                        showText: true
                    },
                    {
                        tap: "nextPage",
                        width: 1 / 3
                    }
                ]
            },
            {
                height: 3 / 10,
                cells: [
                    {
                        tap: "prevPage",
                        width: 1 / 2,
                        showText: true
                    },
                    {
                        tap: "nextPage",
                        width: 1 / 2,
                        showText: true
                    }
                ]
            },
            {
                height: 1 / 10,
                cells: [
                    {
                        tap: "sync",
                        width: 1 / 3,
                        showText: true
                    },
                    {
                        tap: "backup",
                        width: 1 / 3,
                        showText: true
                    },
                    {
                        tap: "bookInfo",
                        width: 1 / 3,
                        showText: true
                    }
                ]
            }
        ];

        this.htmlHelper.setFontSize();
        this.htmlHelper.setWidth();
        this.htmlHelper.setHeight();
        this.htmlHelper.setMargin();
        this.htmlHelper.setNightMode();

        this.setUpColumns();
        this.setUpEvents();

        this.initializeLinkListener();

        setTimeout(function () {
            Ebook.refreshStatusPanelClock();
        }, 60000);
    },
    initializeLinkListener: function() {

        $(document).on("click", "a",
            function(e) {
                e.stopImmediatePropagation();
                e.stopPropagation();
                e.preventDefault();
                e.cancelBubble = true;

                var href = this.getAttribute("href");
                if (href) {
                    if (href.startsWith("#")) {
                        Ebook.goToMarker(href.slice(1));
                    } else {
                        Ebook.messagesHelper.sendLinkClicked(href);
                    }
                }

                return false;
            });
    },
    refreshStatusPanelClock: function () {
        var d = new Date();
        var h = d.getHours();
        var m = d.getMinutes();
        var s = "";
        s += (h < 10 ? "0" : "") + h;
        s += (m < 10 ? "0" : "") + m;
        Ebook.setStatusPanelValues({ "clock": s });
    },
    log: function (msg) {
        return;
        $("#log").show();
        $("#log").prepend($("<div>" + msg + "</div>"));
    },
    generateStatusPanels: function() {

        if (!this.statusPanelItems) this.statusPanelItems = {};
        for (var key in this.statusPanelItems) {
            this.statusPanelItems[key].containers = [];
        }

        this.generateStatusPanel("top");
        this.generateStatusPanel("bottom");
    },
    generateStatusPanel: function (panelId) {
        var items = this.statusPanelItems || {};
        $("#status-panel-" + panelId).html("");

        for (var i in this.statusPanel[panelId]) {
            var cell = this.statusPanel[panelId][i];
            var htmlCell = $("<div class='cell'></div>");
            for (var j in cell) {

                var htmlItem = $("<div class='item'>" + cell[j] + "</div>");
                htmlCell.append(htmlItem);

                if (!items[cell[j]])
                    items[cell[j]] = { value: null, containers: [htmlItem] };
                else {
                    items[cell[j]].containers.push(htmlItem);
                    htmlItem.html(items[cell[j]].value);
                }
            }

            $("#status-panel-" + panelId).append(htmlCell);
        }

        this.statusPanelItems = items;
    },
    setStatusPanelValues: function (keyValues) {
        if (!this.statusPanelItems) this.statusPanelItems = {};

        for (var key in keyValues) {
            var item = this.statusPanelItems[key];
            if (!item) continue;
            item.value = keyValues[key];
            for (var i in item.containers)
                item.containers[i].html(keyValues[key]);
        }
    },
    visualizeCommandCells: function () {
        if ($(".command-cells").length) {
            $(".command-cells").remove();
            return;
        }

        Ebook.getCurrentPosition();

        //TODO: Make sure colors are nice
        //TODO: Test new color system. Extract this function into utility class that can be used in a separate web view for configuring the grid

        var colors = [
            "rgba(255, 206, 95, 0.85)",
            "rgba(215, 236, 95, 0.85)",
            "rgba(255, 176, 95, 0.85)",
            "rgba(255, 46, 95, 0.85)",
            "rgba(255, 106, 95, 0.85)",
            "rgba(205, 206, 95, 0.85)",
            "rgba(155, 206, 95, 0.85)",
            "rgba(55, 206, 95, 0.85)",
            "rgba(255, 206, 105, 0.85)",
            "rgba(255, 206, 235, 0.85)"
        ];

        var commandColors = {};

        var grid = Ebook.commands;
        var visualization = $("<div>").addClass("command-cells");
        for (var i = 0; i < grid.length; i++) {
            var h = Ebook.webViewHeight * grid[i].height;
            var row = $("<div>").addClass("row").css("height", h + "px");
            for (var j = 0; j < grid[i].cells.length; j++) {

                var cell = grid[i].cells[j];
                var w = Ebook.webViewWidth * cell.width;
                var cmd = cell["tap"] || cell["press"];

                if (!commandColors[cmd])
                    commandColors[cmd] = colors[Object.keys(commandColors).length % colors.length];

                var viz = $("<div>")
                    .addClass("cell")
                    .css("width", w + "px")
                    .css("background-color", commandColors[cmd]);

                if (cell.showText)
                    viz.append($("<div>").html("<span class='header'>Tap:</span><br />" + cell["tap"] + "<br /><br />" + "<span class='header'>Long press:</span><br />" + cell["press"]));

                row.append(viz);
            }
            visualization.append(row);
        }

        $("body").append(visualization);
    },
    getCommandCell: function(center) {
        var grid = Ebook.commands;

        var y = 0;
        for (var i = 0; i < grid.length; i++) {

            var row = grid[i];
            y += Ebook.webViewHeight * row.height;

            if (center.y <= y) {

                var x = 0;
                for (var j = 0; j < row.cells.length; j++) {

                    var cell = row.cells[j];
                    x += Ebook.webViewWidth * cell.width;
                    if (center.x <= x)
                        return cell;
                }
            }
        }

        return null;
    },
    performCommand: function(cmd) {
        Ebook.messagesHelper.sendDebug("Touch command: " + cmd);
        switch (cmd) {
        case "nextPage":
            Ebook.goToNextPage(true);
            break;
        case "prevPage":
            Ebook.goToPreviousPage(true);
            break;
        case "visualizeCommandCells":
            Ebook.visualizeCommandCells();
            break;
        case "openQuickSettings":
            Ebook.messagesHelper.sendOpenQuickPanelRequest();
            break;
        default:
            Messages.send("CommandRequest", { Command: cmd });
            break;
        }
    },
    setUpEvents: function() {
        var wrapper = document.getElementsByTagName("body")[0];

        Gestures.init(wrapper);
        KeyStrokes.init(wrapper);
    },
    setUpEbook: function() {
        this.resizeImages();

        this.totalPages = this.getPageCount();
    },
    getPageCount: function() {
        return this.getPageOfMarker("js-ebook-end-of-chapter");
    },
    setUpColumns: function() {
        var columnsInner = document.getElementById("columns-inner");

        var rect = columnsInner.getBoundingClientRect();
        this.pageWidth = rect.width;
        this.pageHeight = rect.height;

        columnsInner.style["column-width"] = this.pageWidth + "px";
    },
    resize: function (width, height) {
        var timer = new CodeTimer();
        Ebook.htmlHelper.hideContent(); timer.time();

        // Use the last position set by a user action if available.
        var position = Ebook.currentPosition || Ebook.getCurrentPosition(); timer.time();

        Ebook.goToPageFast(1); timer.time();
        Ebook.webViewWidth = width; timer.time();
        Ebook.webViewHeight = height; timer.time();
        Ebook.htmlHelper.setWidth(); timer.time();
        Ebook.htmlHelper.setHeight(); timer.time();

        Ebook.setUpColumns(); timer.time();
        Ebook.setUpEbook(); timer.time();

        Ebook.goToPositionFast(position); timer.time();
        Ebook.htmlHelper.showContent(); timer.time();

        timer.sendDebug("resize");
    },
    changeFontSize: function(fontSize) {
        Ebook.htmlHelper.hideContent();
        var position = Ebook.getCurrentPosition();

        Ebook.goToPageFast(1);
        Ebook.fontSize = fontSize;
        Ebook.htmlHelper.setFontSize();

        Ebook.setUpColumns();
        Ebook.setUpEbook();

        Ebook.goToPositionFast(position);
        Ebook.htmlHelper.showContent();
    },
    changeMargin: function(margin) {
        Ebook.htmlHelper.hideContent();
        var position = Ebook.getCurrentPosition();

        Ebook.goToPageFast(1);
        Ebook.webViewMargin = margin;
        Ebook.htmlHelper.setWidth();
        Ebook.htmlHelper.setHeight();
        Ebook.htmlHelper.setMargin();

        Ebook.setUpColumns();
        Ebook.setUpEbook();

        Ebook.goToPositionFast(position);
        Ebook.htmlHelper.showContent();
    },
    goToNextPage: function (saveNewPosition) {
        var page = this.currentPage + 1;
        if (page <= this.totalPages) {
            this.goToPage(page, null, saveNewPosition);
        } else {
            this.messagesHelper.nextChapterRequest();
        }
    },
    goToPreviousPage: function(saveNewPosition) {
        var page = this.currentPage - 1;
        if (page >= 1) {
            this.goToPage(page, null, saveNewPosition);
        } else {
            this.messagesHelper.prevChapterRequest();
        }
    },
    goToPage: function(page, duration, saveNewPosition) {
        if (duration === undefined || duration === null) {
            duration = Ebook.scrollSpeed;
        }

        this.goToPageInternal(page, duration, function () {
            if (saveNewPosition)
                Ebook.currentPosition = Ebook.getCurrentPosition();
            Ebook.messagesHelper.sendPageChange();
        });
    },
    goToPageFast: function(page) {
        this.goToPageInternal(page, 0);
    },
    goToPageInternal: function(page, duration, callback) {
        if (page < 1) {
            page = 1;
        }

        this.currentPage = page;

        if (duration < 1) {
            $('#columns-outer').scrollLeft((page - 1) * this.pageWidth);
            if (callback) callback();
        } else {
            $('#columns-outer').animate({
                scrollLeft: (page - 1) * this.pageWidth,
            }, duration, function () {
                    if (callback) callback();
            });
        }
    },
    goToPosition: function (position, duration) {

        // Insert a marker into the HTML code at the given position.
        var content = document.getElementById("content");
        var html = content.innerHTML;
        html = html.substring(0, position) + "<span id='jr-go-to-position'></span>" + html.substring(position);
        content.innerHTML = html;

        // Now select the inserted marker
        var elmnt = document.getElementById("jr-go-to-position");
        if (!elmnt) {
            Ebook.messagesHelper.sendDebug("Failed to locate position marker for position " + position);
            return;
        }

        // Scroll to the first page to reset the scrolling
        this.goToPageFast(1);

        // Find the marker's left coordinate, and remove it from the HTML code.
        var left = elmnt.getBoundingClientRect().left;
        elmnt.remove();

        // Calculate which page this coordinate is on, 
        var page = Math.trunc(left / this.pageWidth + 1);

        // and go to that page.
        this.goToPage(page, duration);
    },
    goToPositionFast: function(position) {
        this.goToPosition(position, 0);
    },
    getCurrentPosition: function () {

        // Get the caret position for the upper left corner of the screen (with a slight margin)
        var range = document.caretRangeFromPoint(Ebook.webViewMargin + 10, Ebook.webViewMargin + 10);

        // Due to the way the caret position is defined (it's inside the innermost HTML element, most
        // often a <p>), we can't use the startOffset property directly (as that's relative to the parent
        // position). Instead, we insert a marker element, 
        var mark = document.createElement("i");
        mark.innerHTML = "[jr-current-position]";
        range.insertNode(mark);

        // and finds the position of the inserted element in the HTML string.
        var html = document.getElementById("content").innerHTML;        
        var pos = html.split("<i>[jr-current-position]</i>")[0].length;

        // Remove the marker, and return the position.
        mark.remove();
        return pos;
    },
    getPageOfMarker: function(marker) {
        var currentPage = this.currentPage;
        this.goToPageFast(1);
        var markerElement = document.getElementById(marker);
        if (!markerElement) {
             Ebook.messagesHelper.sendDebug("Failed to find marker: " + marker);
             return 0;
        }
        var position = markerElement.getBoundingClientRect().left;
        this.goToPageFast(currentPage);
        return Math.ceil(position / this.pageWidth);
    },
    goToMarker: function(marker, duration) {
        var page = this.getPageOfMarker(marker);
        if (page > 0) {
            this.goToPage(page, duration);
        }
    },
    resizeImages: function() {
        $("img").css("max-width", (Ebook.webViewWidth - (2 * Ebook.webViewMargin)) + "px");
        $("img").css("max-height", (Ebook.webViewHeight - (2 * Ebook.webViewMargin)) + "px");
    },

    panEventCounter: 0,
    panEventHandler: function (e) {

        if (Ebook.panEventCounter < 1) {
            Messages.send("Interaction", { type: "panupdown" });
        }

        if (Ebook.panEventCounter % 10 === 0 || e.isFinal) {
            Ebook.messagesHelper.sendPanEvent(e.center.x, e.center.y);
        }

        Ebook.panEventCounter++;

        if (e.isFinal) {
            Ebook.panEventCounter = 0;
        }
    },
    htmlHelper: {
        setFontSize: function() {
            $("body").css({
                "font-size": Ebook.fontSize + "px"
            });
        },
        setWidth: function() {
            $("#columns-outer").css("width", (Ebook.webViewWidth - (2 * Ebook.webViewMargin)) + "px");
        },
        setHeight: function() {
            $("#columns-outer").css("height", (Ebook.webViewHeight - (2 * Ebook.webViewMargin)) + "px");
        },
        setMargin: function() {
            $("#columns-outer").css({
                "left": Ebook.webViewMargin + "px",
                "top": Ebook.webViewMargin + "px"
            });
        },
        showContent: function() {
            $("#content").css("opacity", 1);
        },
        hideContent: function() {
            $("#content").css("opacity", 0);
        },
        setNightMode: function() {
            $("body").css({
                "background-color": Ebook.nightMode ? "#181819" : "#ffffff",
                "color": Ebook.nightMode ? "#eff2f7" : "#000000"
            });
        },
    },
    messagesHelper: {
        sendPageChange: function () {

            Ebook.setStatusPanelValues({
                "chapterProgress": Ebook.currentPage + " / " + Ebook.totalPages,
                "position": Ebook.currentPosition
            });

            Messages.send("PageChange",
                {
                    CurrentPage: Ebook.currentPage,
                    TotalPages: Ebook.totalPages,
                    Position: Ebook.currentPosition,
                });
        },
        nextChapterRequest: function() {
            Messages.send("NextChapterRequest");
        },
        prevChapterRequest: function() {
            Messages.send("PrevChapterRequest");
        },
        sendOpenQuickPanelRequest: function() {
            Messages.send("OpenQuickPanelRequest");
        },
        sendLinkClicked: function(href) {
            Messages.send("LinkClicked",
                {
                    Href: href
                });
        },
        sendPanEvent: function(x, y) {
            Messages.send("PanEvent",
                {
                    X: x,
                    Y: y
                });
        },
        sendKeyStroke: function(keyCode) {
            Messages.send("KeyStroke",
                {
                    KeyCode: keyCode
                });
        },
        sendDebug: function(data) {
            if (Ebook.debugging)
                Messages.send("Debug", JSON.stringify(data));
        }
    },
};
window.Messages = {
    send: function(action, data) {
        var json = JSON.stringify({
            action: action,
            data: data || {}
        });

        csCallback(Base64.encode(json));
        Ebook.log("OUT: " + action);
    },
    parse: function(data) {
        var json = JSON.parse(Base64.decode(data));
        var res = this.actions[json.Action](json.Data);
        Ebook.log("IN: " + json.Action);
        return res || "";
    },
    actions: {
        init: function(data) {
            Ebook.init(data.Width,
                data.Height,
                data.Margin,
                data.FontSize,
                data.ScrollSpeed,
                data.DoubleSwipe,
                data.NightMode);

            if (data.StatusPanelData)
                this.setStatusPanelData(data.StatusPanelData);

            return performance.now() - pageLoadTime;
        },
        getPageCount: function() {
            return Ebook.getPageCount();
        },
        setStatusPanelData: function (data) {
            if (data.PanelDefinition) {
                Ebook.statusPanel = data.PanelDefinition;
                Ebook.generateStatusPanels();
            }
            if (data.Values)
                Ebook.setStatusPanelValues(data.Values);
            if (!data.PanelDefinition && !data.Values)
                Ebook.setStatusPanelValues(data);
        },
        loadHtml: function(data) {
            Ebook.htmlHelper.hideContent();

            document.getElementById("content").innerHTML = data.Html;

            Ebook.setUpEbook();
            Ebook.setStatusPanelValues({ "chapterTitle": data.Title });

            if (data.Position > 0) {
                Ebook.goToPositionFast(data.Position);
            } else if (data.LastPage) {
                Ebook.goToPageFast(Ebook.totalPages);
                Ebook.messagesHelper.sendPageChange();
            } else if (data.Marker) {
                Ebook.goToMarker(data.Marker);
            } else {
                Ebook.goToPageFast(1);
                Ebook.messagesHelper.sendPageChange();
            }

            Ebook.currentPosition = Ebook.getCurrentPosition();
        
            Ebook.htmlHelper.showContent();
            Ebook.isLoaded = true;

            return performance.now() - pageLoadTime;
        },
        goToPosition: function(data) {
            Ebook.goToPositionFast(data.Position);
        },
        changeFontSize: function(data) {
            Ebook.changeFontSize(data.FontSize);
        },
        resize: function (data) {
            Ebook.resize(data.Width, data.Height);
        },
        changeMargin: function(data) {
            Ebook.changeMargin(data.Margin);
        },
        goToPage: function(data) {
            if (data.Page > 0) {
                Ebook.goToPage(data.Page);
            } else if (data.Next) {
                Ebook.goToNextPage();
            } else if (data.Previous) {
                Ebook.goToPreviousPage();
            }
        },
        setNightMode: function(data) {
            Ebook.nightMode = data.NightMode;
            Ebook.htmlHelper.setNightMode();
        },
        setTheme: function(data) {
            Ebook.setTheme(data);
        }
    },
};

window.Gestures = {
    init: function(element) {
        var hammer = new Hammer.Manager(element);

        hammer.add([
            new Hammer.Tap({
                event: "singletap",
            }),
            new Hammer.Press({
                event: "press",
            }),
            new Hammer.Pan({
                event: "panleft",
                direction: Hammer.DIRECTION_LEFT
            }),
            new Hammer.Pan({
                event: "panright",
                direction: Hammer.DIRECTION_RIGHT
            }),
            new Hammer.Swipe({
                event: "swipeleftdouble",
                direction: Hammer.DIRECTION_LEFT,
                pointers: 2
            }),
            new Hammer.Swipe({
                event: "swiperightdouble",
                direction: Hammer.DIRECTION_RIGHT,
                pointers: 2
            }),
            new Hammer.Pan({
                event: "pandown",
                direction: Hammer.DIRECTION_DOWN
            }),
            new Hammer.Pan({
                event: "panup",
                direction: Hammer.DIRECTION_UP
            })
        ]);

        function perform(action, center) {
            if (!Ebook.isLoaded) return;
            var cell = Ebook.getCommandCell(center);
            if (!cell) return;
            if (!cell[action]) return;
            Ebook.performCommand(cell[action]);
        }

        hammer.on("singletap", function (e) {

            var cell = Ebook.getCommandCell(e.center);
            if(!cell || !cell["tap"] || cell["tap"] !="toggleFullscreen")
                Messages.send("Interaction", { type: "tap" });

            perform("tap", e.center);
        });

        hammer.on("press", function(e) {
            Messages.send("Interaction", { type: "press" });
            perform("press", e.center);
        });

        hammer.on("panleft", function(e) {
            if (!e.isFinal || !Ebook.isLoaded) return;
            Messages.send("Interaction", { type: "panleft" });
            Ebook.goToNextPage(true);
        });

        hammer.on("panright", function(e) {
            if (!e.isFinal || !Ebook.isLoaded) return;
            Messages.send("Interaction", { type: "panright" });
            Ebook.goToPreviousPage(true);
        });

        hammer.on("swipeleftdouble", function() {
            if (!Ebook.isLoaded) return;
            Messages.send("Interaction", { type: "swipeleftdouble" });
            if (Ebook.doubleSwipe) {
                Ebook.messagesHelper.nextChapterRequest();
            }
        });

        hammer.on("swiperightdouble", function() {
            if (!Ebook.isLoaded) return;
            Messages.send("Interaction", { type: "swiperightdouble" });
            if (Ebook.doubleSwipe) {
                Ebook.goToPage(1);
            }
        });

        hammer.on("pandown", Ebook.panEventHandler);
        hammer.on("panup", Ebook.panEventHandler);
    }
};

window.KeyStrokes = {
    init: function(element) {
        element.addEventListener("keydown",
            function(e) {
                Ebook.messagesHelper.sendKeyStroke(e.which || e.keyCode);
            });
    },
};