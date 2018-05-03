
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
    const message = queue.put({ now: paramters.now, msg: paramters.queueMessage.MessageText, visibilityTimeout: paramters.visibilityTimeout, messageTtl: paramters.messageTtl })
    const model = new QueueMessagesListXmlModel()
    model.add(new QueueMessageXmlModel(
      {
        messageId: message.messageId,
        expirationTime: new Date(message.expirationTime * 1000).toUTCString(),
        insertionTime: new Date(message.insertionTime * 1000).toUTCString(),
        popReceipt: message.popReceipt,
        timeNextVisible: new Date(message.timeNextVisible * 1000).toUTCString()
      }))
    let jsBody = model.toJs()
    const response = new AzuriteQueueResponse()
    response.addHttpProperty(N.CONTENT_TYPE, 'application/json')
    res.set(response.httpProps)
    console.log(jsBody)
    res.status(201).send(jsBody)
  }
}
module.exports = new Enqueue()
