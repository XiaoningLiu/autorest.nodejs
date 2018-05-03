const QueueManager = require('./../../providers/core/queue/QueueManager'),
  QueueMessagesListXmlModel = require('./../../providers/xml/queue/QueueMessageList').QueueMessageListXmlModel,
  QueueMessageXmlModel = require('./../../providers/xml/queue/QueueMessageList').QueueMessageXmlModel,
  AzuriteQueueResponse = require('./../../providers/model/queue/AzuriteQueueResponse'),
  N = require('./../../providers/core/HttpHeaderNames')

class Dequeue {
  constructor () {
  }
  process (paramters, res) {
    paramters.numOfMessages = paramters.numOfMessages || 1
    paramters.visibilityTimeout = paramters.visibilityTimeout || 30
    const queue = QueueManager.getQueueAndMessage({ queueName: paramters.queueName }).queue
    const messages = queue.gett({ numOfMessages: paramters.numOfMessages, visibilityTimeout: paramters.visibilityTimeout })
    const model = new QueueMessagesListXmlModel()
    for (const msg of messages) {
      model.add(new QueueMessageXmlModel({
        messageId: msg.messageId,
        expirationTime: new Date(msg.expirationTime * 1000).toUTCString(),
        insertionTime: new Date(msg.insertionTime * 1000).toUTCString(),
        popReceipt: msg.popReceipt,
        timeNextVisible: new Date(msg.timeNextVisible * 1000).toUTCString(),
        dequeueCount: msg.dequeueCount,
        messageText: msg.msg
      }))
    }
    let jsBody = model.toJs()
    const response = new AzuriteQueueResponse()
    response.addHttpProperty(N.CONTENT_TYPE, 'application/xml')
    res.set(response.httpProps)
    console.log(jsBody)
    res.status(200).send(jsBody)
  }
}
module.exports = new Dequeue()
