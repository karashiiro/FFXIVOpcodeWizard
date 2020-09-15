# FFXIVOpcodeWizard
An opcode detection and identification program for FFXIV. This doubles as a working dictionary of the game's opcodes and how to create their conditions. The purpose of this program is to automate the largely procedural reviewing of packet data in order to streamline tool updates.

## Contributing
Want to add a packet scanner? Just add a new `RegisterScanner` call to the scanner registry. The `//=================` dividers represent new conditions; scanners that rely on the behavior of previous scanners should be grouped with those scanners, without dividers. As an example, refer to the market board packets.

For the foreseeable future, we use Sapphire's names for packet types. If you believe a packet name should be changed, please make that request on the [Sapphire repository](https://github.com/SapphireServer/Sapphire). If you have discovered a new packet, please submit it to both Sapphire's `develop` branch, and to this repository.
