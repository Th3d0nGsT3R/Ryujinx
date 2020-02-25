﻿namespace ARMeilleure.Decoders
{
    class OpCode32SimdMemMult : OpCode32
    {
        public int Rn { get; private set; }
        public int Vd { get; private set; }

        public int RegisterRange { get; private set; }
        public int Offset { get; private set; }
        public int PostOffset { get; private set; }
        public bool IsLoad { get; private set; }
        public bool DoubleWidth { get; private set; }
        public bool Add { get; private set; }

        public OpCode32SimdMemMult(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rn = (opCode >> 16) & 0xf;

            bool isLoad = (opCode & (1 << 20)) != 0;
            bool w = (opCode & (1 << 21)) != 0;
            bool u = (opCode & (1 << 23)) != 0;
            bool p = (opCode & (1 << 24)) != 0;

            if (p == u && w)
            {
                Instruction = InstDescriptor.Undefined;
                return;
            }

            DoubleWidth = (opCode & (1 << 8)) != 0;

            if (!DoubleWidth)
            {
                Vd = ((opCode >> 22) & 0x1) | ((opCode >> 11) & 0x1e);
            }
            else
            {
                Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
            }

            Add = u;

            RegisterRange = opCode & 0xff;

            int regsSize = RegisterRange * 4; // Double mode is still measured in single register size.

            if (!u)
            {
                Offset -= regsSize;
            }

            if (w)
            {
                PostOffset = u ? regsSize : -regsSize;
            }
            else
            {
                PostOffset = 0;
            }

            IsLoad = isLoad;

            int regs = DoubleWidth ? RegisterRange / 2 : RegisterRange;

            if (RegisterRange == 0 || RegisterRange > 32 || Vd + regs > 32)
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
