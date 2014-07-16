/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Threading;
using MbUnit.Framework;
using Gallio.Common;
using System.Collections.Concurrent;

namespace Apache.NMS.ZMQ
{
	[TestFixture]
	public class ProducerConsumers : BaseTest
	{
        /*
        [Test]
        public void TempTest()
        {
            System.Diagnostics.Debugger.Launch();

            var factory = NMSConnectionFactory.CreateConnectionFactory(new Uri("zmq:tcp://localhost:5556"));

            using (var connection = factory.CreateConnection())
            {
                using (var session = connection.CreateSession())
                {
                    var destination = session.GetDestination("queue://ZMQTestQueue");
                    using(destination as IDisposable)
                    {
                        var consumers = new IMessageConsumer[3];

                        try
                        {
                            using (var producer = session.CreateProducer(destination))
                            {
                                var queue = new BlockingCollection<IMessage>();

                                for (int i = 0; i < 3; i++)
                                {
                                    int consumerId = i;

                                    var consumer = session.CreateConsumer(destination);
                                    consumer.Listener += (message) =>
                                    {
                                        Tracer.InfoFormat("Consumer {0} received a message.", consumerId);
                                        queue.Add(message);
                                    };

                                    consumers[i] = consumer;
                                }

                                for (int i = 0; i < 10; i++)
                                    producer.Send(session.CreateTextMessage(string.Format("Test Message #{0}", i)));

                                for (int i = 0; i < 10; i++)
                                {
                                    IMessage message = null;

                                    if (queue.TryTake(out message, TimeSpan.FromSeconds(60)))
                                        Tracer.InfoFormat("Message received: {0}", (message as ITextMessage).Text);
                                }
                            }
                        }
                        finally
                        {
                            foreach (var consumer in consumers)
                                consumer.Dispose();
                        }
                    }
                }
            }
        }
        */

		[Test]
		public void TestMultipleProducersConsumer(
            [Column("queue://ZMQTestQueue", "topic://ZMQTestTopic", "temp-queue://ZMQTempQueue", "temp-topic://ZMQTempTopic")]
            string destination,
            [Column(1, 3)]
            int numProducers,
            [Column(1, 3)]
            int numConsumers)
		{
            IConnectionFactory factory = NMSConnectionFactory.CreateConnectionFactory(new Uri("zmq:tcp://localhost:5556"));
            Assert.IsNotNull(factory, "Error creating connection factory.");

			using(IConnection connection = factory.CreateConnection())
			{
				Assert.IsNotNull(connection, "Problem creating connection class. Usually problem with libzmq and clrzmq ");
                using (ISession session = connection.CreateSession())
                {
                    Assert.IsNotNull(session, "Error creating Session.");
                    //using(IDestination testDestination = session.GetDestination(destination))
                    //{
                    IDestination testDestination = session.GetDestination(destination);

                    using (testDestination as IDisposable)
                    {
                        Assert.IsNotNull(testDestination, "Error creating test destination: {0}", destination);

                        // Track the number of messages we should receive
                        int totalMsgCountToReceive = numProducers * numConsumers;

                        IMessageConsumer[] consumers = null;
                        IMessageProducer[] producers = null;

                        var queue = new BlockingCollection<ITextMessage>();

                        try
                        {
                            // Create the consumers
                            consumers = new IMessageConsumer[numConsumers];
                            for (int index = 0; index < numConsumers; index++)
                            {
                                IMessageConsumer consumer = session.CreateConsumer(testDestination);
                                Assert.IsNotNull(consumer, "Error creating consumer on {0}", testDestination.ToString());

                                consumer.Listener += (message) =>
                                    {
                                        Assert.IsInstanceOfType<TextMessage>(message, "Wrong message type received.");
                                        ITextMessage textMsg = (ITextMessage)message;
                                        Assert.AreEqual(textMsg.Text, "Zero Message.");

                                        queue.Add(textMsg);
                                    };
                                consumers[index] = consumer;
                            }

                            // Create the producers
                            producers = new IMessageProducer[numProducers];
                            for (int index = 0; index < numProducers; index++)
                            {
                                producers[index] = session.CreateProducer(testDestination);
                                Assert.IsNotNull(producers[index], "Error creating producer #{0} on {1}", index, destination);
                            }

                            // Sleep for 1 second to wait for initialization
                            Thread.Sleep(1000);

                            // Send the messages
                            for (int index = 0; index < numProducers; index++)
                            {
                                ITextMessage testMsg = session.CreateTextMessage("Zero Message.");
                                Assert.IsNotNull(testMsg, "Error creating test message for producer #{0}.", index);
                                producers[index].Send(testMsg);
                            }

                            TimeSpan maxWaitTime = TimeSpan.FromSeconds(5);

                            int numMsgReceived = 0;

                            while (numMsgReceived < totalMsgCountToReceive)
                            {
                                ITextMessage msg;

                                // Wait for the message
                                if (queue.TryTake(out msg, maxWaitTime))
                                    numMsgReceived++;
                                else
                                    Assert.Fail("Timeout waiting for message receive.");
                            }

                            // Sleep for an extra 2 seconds to see if any extra messages get delivered
                            Thread.Sleep(2 * 1000);
                            Assert.AreEqual(totalMsgCountToReceive, numMsgReceived, "Received too many messages.");
                        }
                        finally
                        {

                            // Clean up the producers
                            if (null != producers)
                            {
                                foreach (IMessageProducer producer in producers)
                                {
                                    producer.Dispose();
                                }
                            }

                            // Clean up the consumers
                            if (null != consumers)
                            {
                                foreach (var consumer in consumers)
                                {
                                    consumer.Dispose();
                                }
                            }
                        }
                    }
                }
			}
		}

		[Test]
        [Ignore]
		private void SingleProducerMultipleDestinations()
		{
			string[] destinations = new string[]
					{
						"queue://ZMQTestQueue1",
						"queue://ZMQTestQueue2",
						"topic://ZMQTestTopic1",
						"topic://ZMQTestTopic2",
						"temp-queue://ZMQTempQueue1",
						"temp-queue://ZMQTempQueue1",
						"temp-topic://ZMQTempTopic1",
						"temp-topic://ZMQTempTopic2"
					};

			// TODO: Create one producer, and then use it to send to multiple destinations.
			Assert.Fail("Not implemented.");
		}
	}
}



