using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace BlackholeBattle
{
    class GameStatePacket : Packet
    {
        public List<Blackhole> Blackholes=new List<Blackhole>();
        public List<GravitationalField> Planets=new List<GravitationalField>();

        public GameStatePacket()
        {
            PacketTypeID = 4;
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
                var planet = new GravitationalField
                {
                    _id = id,
                    mass = mass,
                    state = new State {v = Vector3.Zero, x = pos}
                };
                Planets.Add(planet);
            }
        }
    }
}