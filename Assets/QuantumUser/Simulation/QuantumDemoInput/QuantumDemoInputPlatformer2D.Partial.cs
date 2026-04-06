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
                var analogDir = FPVector2.Rotate(FPVector2.Up, angle * FP.Deg2Rad);

                // Convert back to digital
                pInput.Direction = new FPVector2(
                    analogDir.X > FP._0_05 ? 1 : (analogDir.X < -FP._0_05 ? -1 : 0),
                    analogDir.Y > FP._0_05 ? 1 : (analogDir.Y < -FP._0_05 ? -1 : 0)
                );
            }

            pInput.Jump = input._a;
            pInput.Dodge = input._b;
            pInput.LightAttack = input._c;
            pInput.HeavyAttack = input._d;
            pInput.Throw = input._r1;

            return pInput;
        }
    }
}