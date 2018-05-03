const xml2js = require('xml2js')

module.exports = {
  xml2js: (xmlString) => {
    return new Promise((resolve, reject) => {
      xml2js.parseString(xmlString, (error, result) => {
        if (error) {
          reject(error)
        } else {
          resolve(result)
        }
      })
    })
  }
}
