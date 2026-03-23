namespace Quantum
{
    using Photon.Deterministic;
    unsafe partial struct QuantumDemoInputPlatformer2D
    {
        public static implicit operator Input(QuantumDemoInputPlatformer2D pInput)
        {
            Input input = default;

            // ENCODE DIRECTION - THIS IS CRITICAL!
            if (pInput.Direction != default)
            {
                var angle = FPVector2.RadiansSigned(FPVector2.Up, pInput.Direction) * FP.Rad2Deg;
                angle = (((angle + 360) % 360) / 2) + 1;
                input.ThumbSticks.Regular->_leftThumbAngle = (byte)(angle.AsInt);
            }

            // For keyboard, we can skip the thumbstick encoding entirely
            // Just map buttons directly
            input._a = pInput.Jump;
            input._b = pInput.Dodge;
            input._c = pInput.LightAttack;
            input._d = pInput.HeavyAttack;
            input._r1 = pInput.Throw;
            //input._r2 = pInput.PickUp;
            //input._l1 = pInput.Taunt1;
            //input._l2 = pInput.Taunt2;
            //input._l3 = pInput.Taunt3;
            //input._r3 = pInput.Taunt4;

            // For keyboard movement, you could use thumbsticks or just ignore
            // and handle movement directly in your systems using pInput.Direction

            return input;
        }

        // Optional: If you need to convert back (rarely needed)
        public static implicit operator QuantumDemoInputPlatformer2D(Input input)
        {
            QuantumDemoInputPlatformer2D pInput = default;

            var encoded = input.ThumbSticks.Regular->_leftThumbAngle;
            if (encoded != default)
            {
                int angle = ((int)encoded - 1) * 2;
                pInput.Direction = FPVector2.Rotate(FPVector2.Up, angle * FP.Deg2Rad);
            }

            pInput.Jump = input._a;
            pInput.Dodge = input._b;
            pInput.LightAttack = input._c;
            pInput.HeavyAttack = input._d;
            pInput.Throw = input._r1;
            //pInput.PickUp = input._r2;
            //pInput.Taunt1 = input._l1;
            //pInput.Taunt2 = input._l2;
            //pInput.Taunt3 = input._l3;
            //pInput.Taunt4 = input._r3;

            return pInput;
        }
    }
}