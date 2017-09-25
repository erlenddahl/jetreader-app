const {
    Chromeless
} = require("chromeless")
const {
    expect
} = require("chai")

const DevToolsLibrary = require("./devtoolslibrary")

describe('When change margin', () => {

    it('should be totalPages set to correct value', async() => {
        const chromeless = new Chromeless()

        await chromeless
            .initDevTools(300, 300, 30, 30)
            .sendLoadHtmlMessage(DevToolsLibrary.generateLoremIpsum())
            .sendChangeMarginMessage(45)

        const readerJS = await chromeless.getReaderJS()

        expect(readerJS.totalPages).to.be.equal(25)

        await chromeless.end()
    })

    it('should be PageChange message received with correct value', async() => {
        const chromeless = new Chromeless()

        await chromeless
            .initDevTools(300, 300, 30, 30)
            .sendLoadHtmlMessage(DevToolsLibrary.generateLoremIpsum())
            .sendChangeMarginMessage(45)

        const lastReceivedMessage = await chromeless.getLastReceivedMessage()

        expect(lastReceivedMessage.data.TotalPages).to.be.equal(25)

        await chromeless.end()
    })

    describe('when page is 2', () => {
        it('should scroll to the same position', async() => {
            const chromeless = new Chromeless()

            await chromeless
                .initDevTools(400, 800, 30, 45)
                .sendLoadHtmlMessage(DevToolsLibrary.generateLoremIpsum())
                .goToPageFast(2)
                .sendChangeMarginMessage(30)

            const currentContent = await chromeless.getReaderContent()

            expect(currentContent).to.be.equal('Nullam rhoncus aliquam metus.Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur? Sed vel lectus. Donec odio tempus molestie, porttitor ut, iaculis quis, sem.Cras elementum. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Integer tempor. Nullam lectus justo, vulputate eget mollis sed, tempor sed magna.')

            await chromeless.end()
        });
    });

    before(() => {
        DevToolsLibrary.init()
    })

})