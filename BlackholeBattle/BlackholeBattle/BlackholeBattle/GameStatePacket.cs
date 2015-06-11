using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using System;

namespace BlackholeBattle
{
    class GameStatePacket : Packet
    {
        //For server retaining purposes
        public Vector3 cameraPosition;
        public Vector3 cameraDirection;
        private static Random randall = new Random();

        //Sent to client
        public long Sequence;
        private static long lastSequence = -1;
        public List<Blackhole> Blackholes=new List<Blackhole>();
        public List<Spheroid> Planets = new List<Spheroid>();

        public GameStatePacket()
        {
            PacketTypeID = 4;
            Sequence = ++lastSequence;
        }


        public override void WritePacketData(List<byte> stream)
        {
            WriteInt(stream, Blackholes.Count);
            foreach (var hole in Blackholes)
            {
                WriteInt(stream, hole.ID());
                stream.Add(hole.Owner() ? (byte)1 : (byte)0);
                WriteVector3(stream, hole.Position());
                WriteDouble(stream, hole.Mass());
            }
            WriteInt(stream, Planets.Count);
            foreach (var planet in Planets)
            {
                WriteInt(stream, planet.ID());
                WriteVector3(stream, planet.Position());
                WriteDouble(stream, planet.Mass());
                WriteDouble(stream, planet.Size());
            }
        }

        public override void ReadPacketData(Stream stream)
        {
            var numBlackHoles = ReadInt(stream);
            for(int i = 0;i<numBlackHoles;i++)
            {
                var id = ReadInt(stream);
                var owner = stream.ReadByte();
                var position = ReadVector3(stream);
                var mass = ReadDouble(stream);
                var blackHole = new Blackhole(owner == 1, mass, position);
                blackHole._id = id;
                Blackholes.Add(blackHole);
            }
            var numPlanets = ReadInt(stream);
            for (int i = 0;i<numPlanets;i++)
            {
                var id = ReadInt(stream);
                var pos = ReadVector3(stream);
                var mass = ReadDouble(stream);
                var size = ReadDouble(stream);
                var randy = randall.Next(1, 8);
                var planet = new Spheroid(pos, Vector3.Zero, mass, size, randall.Next(2, 40), randy == 1 ? "earth" : randy == 2 ? "mars" : randy == 3 ? "moon" : randy == 4 ? "neptune" : randy == 5 ? "uranus" : randy == 6 ? "venus" : "ganymede", null);
                planet._id = id;
                Planets.Add(planet);
            }
        }
    }
}