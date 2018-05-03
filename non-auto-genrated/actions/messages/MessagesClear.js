
class Clear {
    constructor() {
    }
    process(paramters, res) {
        res.status(204).send();
    }
}
module.exports = new Clear()
