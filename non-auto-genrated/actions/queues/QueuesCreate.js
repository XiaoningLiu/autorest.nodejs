const QueueManager = require('./../../providers/core/queue/QueueManager'),
  AzuriteQueueResponse = require('./../../providers/model/queue/AzuriteQueueResponse')

class Create {
  constructor () {
  }
  process (paramters, res) {
    const { queue } = QueueManager.getQueueAndMessage({ queueName: paramters.queueName })
    const response = new AzuriteQueueResponse()
    res.set(response.httpProps)
    if (queue !== undefined) {
            // Queue already exists, and existing metadata is identical to the metadata specified on the Create Queue request
      res.status(204).send()
      return
    }

    QueueManager.add({ name: paramters.queueName, metaProps: JSON.parse(paramters.metadata) })
    res.status(201).send()
  }
}
module.exports = new Create()
