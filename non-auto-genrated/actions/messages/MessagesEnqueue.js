
const QueueManager = require('./../../providers/core/queue/QueueManager'),
  QueueMessagesListXmlModel = require('./../../providers/xml/queue/QueueMessageList').QueueMessageListXmlModel,
  QueueMessageXmlModel = require('./../../providers/xml/queue/QueueMessageList').QueueMessageXmlModel,
  AzuriteQueueResponse = require('./../../providers/model/queue/AzuriteQueueResponse'),
  N = require('./../../providers/core/HttpHeaderNames')

class Enqueue {
  constructor () {
  }
  process (paramters, res) {
    paramters.visibilityTimeout = paramters.visibilityTimeout || 0
    paramters.messageTtl = paramters.messageTtl || 60 * 60 * 24 * 7
    const { queue } = QueueManager.getQueueAndMessage({ queueName: paramters.queueName })
    const message = queue.put({ now: paramters.now, msg: paramters.queueMessage.QueueMessage.MessageText, visibilityTimeout: paramters.visibilityTimeout, messageTtl: paramters.messageTtl })
    const model = new QueueMessagesListXmlModel()
    model.add(new QueueMessageXmlModel(
      {
        messageId: message.messageId,
        expirationTime: new Date(message.expirationTime * 1000).toUTCString(),
        insertionTime: new Date(message.insertionTime * 1000).toUTCString(),
        popReceipt: message.popReceipt,
        timeNextVisible: new Date(message.timeNextVisible * 1000).toUTCString()
      }))
    const xmlBody = model.toXml()
    const response = new AzuriteQueueResponse()
    response.addHttpProperty(N.CONTENT_TYPE, 'application/xml')
    res.set(response.httpProps)
    res.status(201).send(xmlBody)
  }
}
module.exports = new Enqueue()
