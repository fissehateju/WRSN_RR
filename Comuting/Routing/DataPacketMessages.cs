using MP.MergedPath.Routing;
using RR.Dataplane;
using RR.Dataplane.NOS;
using RR.Intilization;
using RR.Comuting.computing;
using System;
using System.Collections.Generic;

namespace RR.Comuting.Routing
{
    

    class DataPacketMessages
    {
        private LoopMechanizimAvoidance loopMechan = new LoopMechanizimAvoidance();
        private NetworkOverheadCounter counter;
        /// <summary>
        /// currentBifSensor: current node that has the packet.
        /// Branches: the branches 
        /// isSourceAnAgent: the source is an agent for a sink. That is to say no need for clustering the source itslef this time.
        /// </summary>
        /// <param name="currentBifSensor"></param>
        /// <param name="Branches"></param>
        /// <param name="packet"></param>
        /// <param name="isSourceAnAgent"></param>
        public DataPacketMessages(Sensor sender, Packet packet)
        {
            counter = new NetworkOverheadCounter();

            // the source node creates new
            if (packet.PacketType == PacketType.ResponseSinkPosition) //  the packet arrived to the source node. it was response. now we will generate of duplicate the packet.
            {
                // create new:
                foreach (SinksAgentsRow row in packet.SinksAgentsList)
                {
                    if (sender.ID == row.AgentNode.ID)
                    {
                        //skip the test here and send to the known sink by urself
                        //Hand of to the sink by urself 
                        Packet pkt = GeneragtePacket(sender, row); //                                                         
                        pkt.SinksAgentsList = packet.SinksAgentsList;
                        HandOffToTheSinkOrRecovry(sender, pkt);

                    }
                    else
                    {
                        Packet pck = GeneragtePacket(sender, row); // duplicate.                                                            
                        pck.SinksAgentsList = packet.SinksAgentsList;
                        SendPacket(sender, pck);
                    }

                }
            }
            else if (packet.PacketType == PacketType.Data)
            {
                // recovery packets:
                Packet dupPck = Duplicate(packet, sender);
                SendPacket(sender, dupPck);
            }


        
    }

        public DataPacketMessages()
        {
            counter = new NetworkOverheadCounter();
        }


        public void HandelInQueuPacket(Sensor currentNode, Packet InQuepacket)
        {
            SendPacket(currentNode, InQuepacket);
        }




        /// <summary>
        /// duplicate the packet. this means no new packet is generated.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="currentBifSensor"></param>
        /// <returns></returns>
        private Packet Duplicate(Packet packet, Sensor currentBifSensor)
        {
            Packet pck = packet.Clone() as Packet;
            return pck;
        }
      

        private Packet GeneragtePacket(Sensor sender, SinksAgentsRow row)
        {
            //Should not enter here if its an agent
            PublicParamerters.NumberofGeneratedPackets += 1;
            Packet pck = new Packet();
            pck.Source = sender;
            pck.Path = "" + sender.ID;
            pck.Destination = row.AgentNode; 
            pck.PacketType = PacketType.Data;
            pck.PID = PublicParamerters.NumberofGeneratedPackets;
            pck.TimeToLive = Convert.ToInt16((Operations.DistanceBetweenTwoPoints(sender.CenterLocation, pck.Destination.CenterLocation) / (PublicParamerters.CommunicationRangeRadius / 3)));
            pck.TimeToLive += PublicParamerters.HopsErrorRange;
            counter.DisplayRefreshAtGenertingPacket(sender, PacketType.Data);
            return pck;
        }

        public void SendPacket(Sensor sender, Packet pck)
        {
            if (pck.PacketType == PacketType.Data)
            {
                // neext hope:
                sender.SwichToActive(); // switch on me.
                Sensor Reciver = SelectNextHop(sender, pck);
                if (Reciver != null)
                {
                    // overhead:
                    counter.ComputeOverhead(pck, EnergyConsumption.Transmit, sender, Reciver);
                    counter.Animate(sender, Reciver, pck);
                    //:
                    Reciver.numOfPacketsFollowing += 1; 
                    RecivePacket(Reciver, pck);
                }
                else
                {
                    counter.SaveToQueue(sender, pck); // save in the queue.
                }
            }
          
        }

        private void RecivePacket(Sensor Reciver, Packet packt)
        {
            packt.Path += ">" + Reciver.ID;

            if (loopMechan.isLoop(packt))
            {
                counter.DropPacket(packt, Reciver, PacketDropedReasons.Loop);
            }
            else
            {
                packt.ReTransmissionTry = 0;
                if (packt.Destination.ID == Reciver.ID)
                {
                    counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                    counter.DisplayRefreshAtReceivingPacket(Reciver);
                    HandOffToTheSinkOrRecovry(Reciver, packt);
                }
                else
                {
                    counter.ComputeOverhead(packt, EnergyConsumption.Recive, null, Reciver);
                    counter.DisplayRefreshAtReceivingPacket(Reciver);
                    if (packt.Hops <= packt.TimeToLive)
                    {
                        SendPacket(Reciver, packt);
                    }
                    else
                    {
                        counter.DropPacket(packt, Reciver, PacketDropedReasons.TimeToLive);
                    }
                }
            }
        }
  


        /// <summary>
        /// find x in inlist
        /// </summary>
        /// <param name="x"></param>
        /// <param name="inlist"></param>
        /// <returns></returns>
        private bool Find(SinksAgentsRow x, List<SinksAgentsRow> inlist)
        {
            foreach (SinksAgentsRow rec in inlist)
            {
                if (rec.Sink.ID == x.Sink.ID)
                {
                    return true;
                }
            }

            return false;
        }

        private List<SinksAgentsRow> GetMySinksFromPacket(Sensor Agent, Packet pck)
        {
            int AgentID = Agent.ID;
            bool isFollowup = pck.isFollowUp;
            List<SinksAgentsRow> inpacketSinks = pck.SinksAgentsList;

            List<SinksAgentsRow> re = new List<SinksAgentsRow>();
            foreach (SinksAgentsRow x in inpacketSinks)
            {
                if (x.AgentNode.ID == AgentID)
                {
                    re.Add(x);
                }
            }
            return re;
        }

        

        /// <summary>
        /// hand the packet to my sink.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="packt"></param>
        private void HandOffToTheSinkOrRecovry(Sensor agent, Packet packt)
        {
           
            // check how many sinks are there in my record
            if (agent != null)
            {
                if (packt.SinksAgentsList != null)
                {
                    // the sinks in the packet:
                    List<SinksAgentsRow> MyinpacketSinks = GetMySinksFromPacket(agent, packt); // The sinks the agent has record of 
                    // the sinks within my range.
                    List<SinksAgentsRow> inAgentList = agent.GetSinksAgentsList;
                    List<int> SinksIDsRequiredRecovery = new List<int>(); // the sinks that required recovery.
                  
                    foreach (SinksAgentsRow rowInpacket in MyinpacketSinks)
                    {
                        // check if sink still within the range of the agent
                        bool isFound = Find(rowInpacket, inAgentList);
                        if (isFound)
                        {
                            //
                            Sink sink = rowInpacket.Sink;
                            packt.Path += "> Sink: " + sink.ID;
                            counter.SuccessedDeliverdPacket(packt);
                        }
                        else
                        {    // the sinks is not within my range:
                            // change packet destination:
                            // keep this for recovery.
                            SinksIDsRequiredRecovery.Add(rowInpacket.Sink.ID);
                        }
                    }

                    // recovery: SinksIDsRequiredRecovery
                    if (SinksIDsRequiredRecovery.Count > 0)
                    {
                        packt.SinkIDsNeedsRecovery = SinksIDsRequiredRecovery;
                        new RecoveryMessage(agent, packt);
                    }
                }
                else
                {
                    // i dont know when it should be null.
                    counter.DropPacket(packt, agent,PacketDropedReasons.InformationError);
                    Console.Write("MergedPathsMessages->HandOffToTheSinkOrRecovry->packt.SinksAgentsList==null");
                }
            }
           
        }


   
        
        /// <summary>
        /// get the max value
        /// </summary>
        /// <param name="ni"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        public Sensor SelectNextHop(Sensor ni, Packet packet)
        {
                return new GreedyRoutingMechansims().RrGreedy(ni, packet);
        }


    }
}
