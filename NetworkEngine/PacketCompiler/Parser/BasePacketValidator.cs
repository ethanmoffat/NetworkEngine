
using System.Collections.Generic;
using System.Linq;
using NetworkEngine.PacketCompiler.State;

namespace NetworkEngine.PacketCompiler.Parser
{
    public class BasePacketValidator : IBasePacketValidator
    {
        public ValidationState ValidatePacketStates(params PacketState[] packetStates)
        {
            var packetNames = new HashSet<string>(packetStates.Select(x => x.PacketName));
            foreach (var packetState in packetStates)
            {
                var packetBase = packetState.BasePacketName;
                if (string.IsNullOrEmpty(packetBase) )
                    continue;

                if (!packetNames.Contains(packetBase))
                {
                    return new ValidationState(ValidationResult.NonexistentBasePacket,
                                               $"Packet {packetState.PacketName} references non-existent base packet {packetBase}");
                }

                var validation = CheckForCircularDependencies(packetStates, packetState);
                if (validation.Status != ValidationResult.Ok)
                    return validation;
            }

            return new ValidationState(ValidationResult.Ok);
        }

        private static ValidationState CheckForCircularDependencies(IEnumerable<PacketState> packetStates, PacketState currentState)
        {
            var namesToStates = packetStates.ToDictionary(k => k.PacketName);
            var dependencies = new HashSet<string>();
            var toProcess = new Queue<PacketState>();

            toProcess.Enqueue(namesToStates[currentState.BasePacketName]);
            while (toProcess.Count > 0)
            {
                var nextItem = toProcess.Dequeue();
                if (!dependencies.Add(nextItem.PacketName))
                {
                    return new ValidationState(ValidationResult.CircularBasePacketDependency,
                                               $"Packet {currentState.PacketName} has a circular dependency in its base packets; {nextItem.PacketName} is depended upon more than once");
                }

                if (!string.IsNullOrEmpty(nextItem.BasePacketName) && namesToStates.ContainsKey(nextItem.BasePacketName))
                {
                    toProcess.Enqueue(namesToStates[nextItem.BasePacketName]);
                }
            }

            return new ValidationState(ValidationResult.Ok);
        }
    }
}