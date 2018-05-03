
class SetProperties {
    constructor() {
    }
    process(paramters, res) {
        res.status(202).send();
    }
}
module.exports = new SetProperties()
