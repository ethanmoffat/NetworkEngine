using NetworkEngine.PacketCompiler.State;

namespace NetworkEngine.PacketCompiler.Parser
{
    public interface IBasePacketValidator
    {
        ValidationState ValidatePacketStates(params PacketState[] packetStates);
    }
}