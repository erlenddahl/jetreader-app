/*global Ebook*/
/*global Messages*/
/*global Base64*/
/*global csCallback*/
/*global Hammer*/
/*global Gestures*/
/*global KeyStrokes*/

if (!Math.trunc) {
    Math.trunc = function (v) {
        return v < 0 ? Math.ceil(v) : Math.floor(v);
    };
}

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
    webViewWidth: 0,
    webViewHeight: 0,
    scrollSpeed: 0,
    doubleSwipe: false,
    debugging: true,
    stats: {
        minPageTime: 3,
        maxPageTime: 120
    },
    init: function (data) {

        this.webViewWidth = data.Width;
        this.webViewHeight = data.Height;
        this.fontSize = data.FontSize;
        this.margins = data.Margins;
        this.scrollSpeed = data.ScrollSpeed;
        this.doubleSwipe = data.DoubleSwipe;
        this.commands = data.Commands;
        this.setTheme(data.Theme);

        this.htmlHelper.setFontSize();
        this.htmlHelper.setWidth();
        this.htmlHelper.setHeight();
        this.htmlHelper.setMargin();

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
        Ebook.setStatusPanelValues({ "Clock": s });
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

        this.generateStatusPanel("Top");
        this.generateStatusPanel("Bottom");
    },
    generateStatusPanel: function (panelId) {
        var items = this.statusPanelItems || {};
        $("#status-panel-" + panelId.toLowerCase()).html("");

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

            $("#status-panel-" + panelId.toLowerCase()).append(htmlCell);
        }

        this.statusPanelItems = items;
    },
    showProgressMessage: function (data) {

        if (data.preset === "Success") {
            data.color = "#d1ffcd";
            data.duration = 3;
            data.icon = '<svg version="1.1" style="height: 8px;" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 512 512" style="enable-background:new 0 0 512 512;" xml:space="preserve"><g><g><path d="M504.502,75.496c-9.997-9.998-26.205-9.998-36.204,0L161.594,382.203L43.702,264.311c-9.997-9.998-26.205-9.997-36.204,0c-9.998,9.997-9.998,26.205,0,36.203l135.994,135.992c9.994,9.997,26.214,9.99,36.204,0L504.502,111.7C514.5,101.703,514.499,85.494,504.502,75.496z"/></g></g></svg>';
        } else if (data.preset === "Failure") {
            data.color = "#ffcfcd";
            data.duration = 3;
            data.icon = '<svg version="1.1" style="height: 8px;" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 512 512" style="enable-background:new 0 0 512 512;" xml:space="preserve"><g><g><path d="M501.362,383.95L320.497,51.474c-29.059-48.921-99.896-48.986-128.994,0L10.647,383.95c-29.706,49.989,6.259,113.291,64.482,113.291h361.736C495.039,497.241,531.068,433.99,501.362,383.95z M256,437.241c-16.538,0-30-13.462-30-30c0-16.538,13.462-30,30-30c16.538,0,30,13.462,30,30C286,423.779,272.538,437.241,256,437.241zM286,317.241c0,16.538-13.462,30-30,30c-16.538,0-30-13.462-30-30v-150c0-16.538,13.462-30,30-30c16.538,0,30,13.462,30,30V317.241z"/></g></g></svg>';
        }

        $("#status-panel-progress .progress-message").html(data.icon + " " + data.message);

        if (data.color) {
            $("#status-panel-progress").css("background-color", data.color);
        } else {
            $("#status-panel-progress").css("background-color", "");
        }

        $("#status-panel-bottom").hide();

        var queue = $("#status-panel-progress").stop(true, true).fadeIn(1);

        if (data.duration && data.duration > 0) {
            queue.animate({opacity:1}, data.duration * 1000).fadeOut(500,
                function() {
                    $("#status-panel-bottom").fadeIn(500);
                });
        }

    },
    hideProgressMessage: function () {
        $("#status-panel-progress").fadeOut(500, function() {
            $("#status-panel-bottom").fadeIn(500);
        });
    },
    setTheme: function(theme) {
        this.theme = theme;
        Ebook.htmlHelper.setColors();
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
    refreshChapterInfo: function() {
        var c = Ebook.chapterInfo;
        if (!c) return;
        var wordsPerPage = c.wordsCurrent / Ebook.totalPages;
        var wordsRead = Ebook.currentPage * wordsPerPage;
        var progress = (c.wordsBefore + wordsRead) / (c.wordsBefore + c.wordsCurrent + c.wordsAfter) * 100.0;
        Ebook.setStatusPanelValues({
            "BookProgressPercentage": progress.toFixed(1) + "%"
        });
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
        case "NextPage":
            Ebook.goToNextPage(true);
            break;
        case "PrevPage":
            Ebook.goToPreviousPage(true);
            break;
        case "VisualizeCommandCells":
            Ebook.visualizeCommandCells();
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
        columnsInner.style["-moz-column-width"] = this.pageWidth + "px";
        columnsInner.style["-webkit-column-width"] = this.pageWidth + "px";
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
    changeFontSize: function(message) {
        Ebook.htmlHelper.hideContent();
        var position = Ebook.getCurrentPosition();

        Ebook.goToPageFast(1);
        Ebook.fontSize = message.FontSize;
        Ebook.htmlHelper.setFontSize();

        Ebook.setUpColumns();
        Ebook.setUpEbook();

        Ebook.goToPositionFast(position);
        Ebook.htmlHelper.showContent();
    },
    changeMargins: function(margins) {
        Ebook.htmlHelper.hideContent();
        var position = Ebook.getCurrentPosition();

        Ebook.goToPageFast(1);
        Ebook.margins = margins;
        Ebook.htmlHelper.setWidth();
        Ebook.htmlHelper.setHeight();
        Ebook.htmlHelper.setMargin();

        Ebook.setUpColumns();
        Ebook.setUpEbook();

        Ebook.goToPositionFast(position);
        Ebook.htmlHelper.showContent();
    },
    reportReadStats: function () {
        if (!this.stats.startPageTime) return;
        var now = Date.now() / 1000;
        var diff = (now - this.stats.startPageTime);
        this.resumeStatsTimer(now);
        this.stats.storedReadingTime = 0;
        if (diff < Ebook.stats.minPageTime || diff > Ebook.stats.maxPageTime) {
            return;
        }

        var c = Ebook.chapterInfo || {};

        if (Ebook.totalPages <= 0) return;

        this.messagesHelper.sendStats({
            Seconds: diff,
            TotalPages: Ebook.totalPages,
            Words: c.wordsCurrent / Ebook.totalPages,
            Progress: (c.wordsBefore + (c.wordsCurrent / Ebook.totalPages) * Ebook.currentPage) / (c.wordsBefore + c.wordsCurrent + c.wordsAfter) * 100
        });
    },
    pauseStatsTime: function () {
        this.stats.storedReadingTime += Date.now() / 1000 - this.stats.startPageTime;
    },
    resumeStatsTimer: function(now) {
        this.stats.startPageTime = now || Date.now() / 1000;
    },
    goToNextPage: function (saveNewPosition) {
        this.reportReadStats();
        var page = this.currentPage + 1;
        if (page <= this.totalPages) {
            this.goToPage(page, null, saveNewPosition);
        } else {
            this.messagesHelper.nextChapterRequest();
        }
    },
    goToPreviousPage: function (saveNewPosition) {
        this.reportReadStats();
        var page = this.currentPage - 1;
        if (page >= 1) {
            this.goToPage(page, null, saveNewPosition);
        } else {
            this.messagesHelper.prevChapterRequest();
        }
    },
    goToPage: function (page, duration, saveNewPosition) {
        this.reportReadStats();
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
    goToPageInternal: function (page, duration, callback) {
        this.reportReadStats();

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

        Ebook.messagesHelper.sendDebug(html.length + ", " + position);

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
        var range = document.caretRangeFromPoint(Ebook.margins.Left + 10, Ebook.margins.Top + 10);

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
        $("img").css("max-width", (Ebook.webViewWidth - (2 * Ebook.margins.Left)) + "px"); //TODO: Crashes on .margins == null when adding and auto-opening new book (Ace Atkins => Kickback)
        $("img").css("max-height", (Ebook.webViewHeight - (2 * Ebook.margins.Top)) + "px");
    },

    panStart: null,
    lastPanEvent: null,
    panEventHandler: function (e) {

        if (!Ebook.panStart) {
            Messages.send("Interaction", { type: "panupdown" });
            Ebook.panStart = e.center;
        }

        if (e.isFinal || !Ebook.lastPanEvent || Math.abs(e.center.x - Ebook.lastPanEvent.x) > 5 || Math.abs(e.center.y - Ebook.lastPanEvent.y) > 5) {
            Ebook.messagesHelper.sendPanEvent(Ebook.panStart, e.center, e.isFinal);
            Ebook.lastPanEvent = e.center;
        }

        if (e.isFinal) {
            Ebook.panStart = null;
            Ebook.lastPanEvent = null;
        }
    },
    htmlHelper: {
        setFontSize: function() {
            $("body").css({
                "font-size": Ebook.fontSize + "px"
            });
        },
        setWidth: function() {
            $("#columns-outer").css("width", (Ebook.webViewWidth - (2 * Ebook.margins.Left)) + "px");
        },
        setHeight: function() {
            $("#columns-outer").css("height", (Ebook.webViewHeight - (2 * Ebook.margins.Top)) + "px");
        },
        setMargin: function() {
            $("#columns-outer").css({
                "left": Ebook.margins.Left + "px",
                "top": Ebook.margins.Top + "px"
            });

            Ebook.messagesHelper.sendDebug($("#columns-outer").css(["width", "height", "left", "top"]));
            Ebook.messagesHelper.sendDebug(Ebook.margins);
        },
        showContent: function() {
            $("#content").css("opacity", 1);
        },
        hideContent: function() {
            $("#content").css("opacity", 0);
        },
        setColors: function() {
            $("body").css({
                "background-color": Ebook.theme.BackgroundColor,
                "color": Ebook.theme.ForegroundColor
            });
        }
    },
    messagesHelper: {
        sendPageChange: function () {

            Ebook.setStatusPanelValues({
                "ChapterProgress": Ebook.currentPage + " / " + Ebook.totalPages,
                "Position": Ebook.currentPosition
            });
            Ebook.refreshChapterInfo();

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
        prevChapterRequest: function () {
            Messages.send("PrevChapterRequest");
        },
        sendStats: function (stats) {
            Messages.send("ReadStats", stats);
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
        sendPanEvent: function(start, current, isFinal) {
            Messages.send("PanEvent",
                {
                    StartX: start.x,
                    StartY: start.y,
                    CurrentX: current.x,
                    CurrentY: current.y,
                    IsFinal: isFinal
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

        Ebook.messagesHelper.sendDebug("Received action: '" + json.Action + "'");

        if (typeof this.actions[json.Action] === 'function')
            return this.actions[json.Action](json.Data) || "";

        if (typeof Ebook[json.Action] === 'function')
            return Ebook[json.Action](json.Data) || "";

        Ebook.log("IN: " + json.Action);

        Ebook.messagesHelper.sendDebug("Missing action: '" + json.Action + "'");
        return "";
    },
    actions: {
        init: function(data) {

            Ebook.init(data);

            if (data.StatusPanelData)
                this.setStatusPanelData(data.StatusPanelData);

            return performance.now() - pageLoadTime;
        },
        setChapterInfo: function(data) {
            Ebook.chapterInfo = data;
            Ebook.refreshChapterInfo();
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
            Ebook.setStatusPanelValues({ "ChapterTitle": data.Title });

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
            Ebook.resumeStatsTimer();

            return performance.now() - pageLoadTime;
        },
        goToPosition: function(data) {
            Ebook.goToPositionFast(data.Position);
        },
        goToPage: function(data) {
            if (data.Page > 0) {
                Ebook.goToPage(data.Page);
            } else if (data.Next) {
                Ebook.goToNextPage();
            } else if (data.Previous) {
                Ebook.goToPreviousPage();
            }
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

        function isLink(e) {
            if (e && e.target) {
                var el = e.target;
                while (el !== null && el.id !== "content") {
                    if (el.localName === "a") return true;
                    el = el.parentElement;
                }
            }
            return false;
        }

        hammer.on("singletap", function (e) {

            if (isLink(e)) return;

            var cell = Ebook.getCommandCell(e.center);
            if(!cell || !cell["tap"] || cell["tap"] != "ToggleFullscreen")
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