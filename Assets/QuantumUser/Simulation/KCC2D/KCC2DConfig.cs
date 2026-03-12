using JetBrains.Annotations;
using Quantum.Physics2D;
using UnityEngine;

namespace Quantum
{
  using Photon.Deterministic;

  public unsafe class KCC2DConfig : AssetObject
  {
    [Header("Shape and Collision")] public FP CapsuleRadius = FP._0_25;
    public FP CapsuleHeight = 1;
    public LayerMask Mask;

    [Space(10)][Header("Depenetration")] public int SolverIterations = 4;
    [RangeEx(0, 1)] public FP IterationCorrectionRate = FP._0_50;
    [RangeEx(0, 0.25)] public FP AllowedPenetration = FP.EN2;
    public bool CCD = true;

    [Space(5)]
    [Header("Horizontal Movement")]
    // movement
    [RangeEx(4, 60)] public FP Acceleration = 10;
    [RangeEx(1, 10)] public FP FlipDirectionMultiplier = 1;
    [RangeEx(4, 60)] public FP Deceleration = 10;
    [RangeEx(1, 10)] public FP MaxBaseSpeed = 4;

    [Space(5)][Header("Dashing")] public DashDirection DirectionType = DashDirection.Input;
    public bool DashSuspendsGravity = true;
    [RangeEx(0.1, 1)] public FP DashDuration = FP._0_25;
    [RangeEx(5, 20)] public FP MaxDashSpeed = 10;

    // gravity and slopes
    [Space(5)]
    [Header("Gravity and Slopes")]
    public FP BaseGravity = -10;
    [RangeEx(1, 4)] public FP DownGravityMultiplier = 1;
    public FP MaxSlopeAngle = 30;
    [RangeEx(4, 25)] public FP SlopeMaxSpeed = 10;
    [RangeEx(10, 100)] public FP FreeFallMaxSpeed = 25;

    [Space(5)][Header("Jumps")] public FP JumpImpulse;
    [RangeEx(0, 1)] public FP AirControlFactor = 1;
    public bool FastFlipOnAir = true;
    public bool DownGravityOnRelease = true;
    [RangeEx(0.05, 0.25)] public FP CoyoteTime = FP._0_10;
    [RangeEx(0.05, 0.25)] public FP InputBufferTime = FP.EN1;
    public bool DoubleJumpEnabled = true;
    public bool DoubleJumpWhenFreeFalling = true;
    [RangeEx(0, 10)] public FP DecelerationOnAir = 5;

    [Space(5)]
    [Header("Wall Jumps")]
    public bool WallJumpEnabled = true;
    public bool RequiresOppositeInput = true;
    [RangeEx(0, 1)] public FP WalledStateExtention = FP._0_25;
    public FP MinWallAngle = 75;
    public FP MaxWallAngle = 100;
    public FPVector2 WallJumpImpulse = new FPVector2(1, 6);
    [RangeEx(1, 10)] public FP WallMaxSpeed = 10;

    // Misc
    [Space(5)][Header("Misc")] public bool Debug = false;
    public ColorRGBA ColorFinal = ColorRGBA.Blue;


    private KCCQueryResult[] _contacts = new KCCQueryResult[16];
    private int _contactsCount = 0;
    private Shape2D _capsuleShape;

    public void Move(Frame f, EntityRef entity, Transform2D* transform, KCC2D* KCC)
    {
      if (KCC->IgnoreStep)
      {
        // skip one time
        KCC->IgnoreStep = false;
        return;
      }

      _capsuleShape = Shape2D.CreateCapsule(CapsuleRadius, CapsuleHeight / 2 - CapsuleRadius);
      var position = transform->Position;

      // based on current state
      IntegrateForces(f, entity, transform, KCC);

      ProcessJump(f, entity, transform, KCC);

      ProcessDash(f, entity, transform, KCC);

      int steps = 1;
      if (CCD)
      {
        var fullStepLength = FPMath.Abs(KCC->CombinedVelocity.Magnitude) * f.DeltaTime;
        steps = (fullStepLength / CapsuleRadius).AsInt + 1;
      }
      for (int step = 0; step < steps; step++)
      {
        KCC->Closest.Overlapping = false;
        KCC->Closest.ContactType = KCCContactType.NONE;
        // pre-move (velocity * delta / steps)
        position += (KCC->CombinedVelocity * f.DeltaTime) / steps;

        // apply movement step
        transform->Position = position;

        KCC->IgnoreStep = false;

        // find contacts (no details yet)
        FindContacts(f, entity, KCC, position);
        if (KCC->IgnoreStep) return;

        // for each solver iteration
        if (_contactsCount > 0)
        {
          for (int s = 0; s < SolverIterations; s++)
          {
            // verify if done for this step (solver)
            if (SolverIteration(f, entity, transform, KCC, ref position, s) == false) break;

            // apply movement corrections
            transform->Position = position;
          }
        }
      }

      if (Debug) Draw.Capsule(position, _capsuleShape.Capsule, color: ColorFinal);

      // switch state
      ComputeState(f, entity, transform, KCC);
    }

    private void ProcessDash(Frame frame, EntityRef entity, Transform2D* transform, KCC2D* kcc)
    {
      if (kcc->State == KCCState.DASHING) return;

      if (kcc->Closest.ContactType == KCCContactType.WALL)
      {
        var oppositeInput = kcc->Closest.Contact.Normal.X * kcc->LastInputDirection < 0;
        if (oppositeInput) return;
      }

      if (kcc->Input.Dash.WasPressed)
      {
        kcc->SetState(frame, KCCState.DASHING, DashDuration);
        switch (DirectionType)
        {
          case DashDirection.Velocity:
            kcc->KinematicHorizontalSpeed = FPMath.Sign(kcc->CombinedVelocity.X) * MaxDashSpeed;
            break;
          case DashDirection.Input:
            kcc->KinematicHorizontalSpeed = kcc->LastInputDirection * MaxDashSpeed;
            break;
        }
        if (DashSuspendsGravity) kcc->KinematicVerticalSpeed = 0;
      }
    }

    private void ProcessJump(Frame f, EntityRef entity, Transform2D* transform, KCC2D* kcc)
    {
      var prematureJump = kcc->GroundedJumpTimer.IsExpiredOrNotValid(f) == false;
      if (kcc->Input.Jump.WasPressed || prematureJump)
      {
        var impulse = new FPVector2(kcc->KinematicHorizontalSpeed, JumpImpulse);
        switch (kcc->State)
        {
          case KCCState.GROUNDED:
            kcc->Jump(f, entity, impulse);
            f.Events.Jumped(entity, KCCState.JUMPED, KCCState.GROUNDED, impulse);
            kcc->SetState(f, KCCState.JUMPED);
            break;
          case KCCState.JUMPED:
            if (DoubleJumpEnabled)
            {
              kcc->Jump(f, entity, impulse);
              f.Events.Jumped(entity, KCCState.DOUBLE_JUMPED, KCCState.JUMPED, impulse);
              kcc->SetState(f, KCCState.DOUBLE_JUMPED, 1);
            }
            else if (kcc->Input.Jump.WasPressed) kcc->GroundedJumpTimer = FrameTimer.FromSeconds(f, InputBufferTime);
            break;
          case KCCState.WALLED:
            FP wallContactDirection = kcc->Closest.Contact.Normal.X > 0 ? 1 : -1;
            impulse = WallJumpImpulse;
            impulse.X *= wallContactDirection;
            kcc->Jump(f, entity, impulse);
            f.Events.Jumped(entity, KCCState.JUMPED, KCCState.WALLED, impulse);
            kcc->SetState(f, KCCState.JUMPED);
            break;
          case KCCState.FREE_FALLING:
            if (DoubleJumpEnabled && DoubleJumpWhenFreeFalling)
            {
              kcc->Jump(f, entity, impulse);
              f.Events.Jumped(entity, KCCState.DOUBLE_JUMPED, KCCState.FREE_FALLING, impulse);
              kcc->SetState(f, KCCState.DOUBLE_JUMPED, 1);
            }
            else if (kcc->Input.Jump.WasPressed) kcc->GroundedJumpTimer = FrameTimer.FromSeconds(f, InputBufferTime);
            break;
          case KCCState.DOUBLE_JUMPED:
            if (kcc->Input.Jump.WasPressed) kcc->GroundedJumpTimer = FrameTimer.FromSeconds(f, InputBufferTime);
            break;

        }
      }
    }

    private void ComputeState(Frame f, EntityRef entity, Transform2D* transform, KCC2D* kcc)
    {
      // grounded and walled have priority and always switch
      var forceSwitch = ShouldForcedSwitch(kcc->State, kcc->Closest.ContactType);
      var previousState = kcc->State;
      // switch state based on contacts
      if (forceSwitch || kcc->StateTimer.IsExpiredOrNotValid(f))
      {
        switch (kcc->Closest.ContactType)
        {
          case KCCContactType.GROUND:
            kcc->SetState(f, KCCState.GROUNDED, forceSwitch ? CoyoteTime : null);
            kcc->_dynamicVelocity *= FPMath.Clamp01(1 - Deceleration * f.DeltaTime);
            //kcc->DynamicVelocity = default;
            break;
          case KCCContactType.WALL:
            if (RequiresOppositeInput)
            {
              var inputDirection = kcc->Input.Left.IsDown ? -1 : kcc->Input.Right.IsDown ? 1 : 0;
              var oppositeInput = kcc->Closest.Contact.Normal.X * inputDirection < 0;
              if (oppositeInput) kcc->SetState(f, KCCState.WALLED);
            }
            else
            {
              kcc->SetState(f, KCCState.WALLED);
            }
            break;
          case KCCContactType.SLOPE:
            kcc->SetState(f, KCCState.SLOPED);
            break;
          case KCCContactType.NONE:
            kcc->SetState(f, KCCState.FREE_FALLING);
            break;
          case KCCContactType.CEIL:
            kcc->SetState(f, KCCState.DOUBLE_JUMPED, 1);
            break;
        }
      }

      // apply stuff based on state
      switch (kcc->State)
      {
        case KCCState.WALLED:
          // keep the state hanging if still in contact
          if (kcc->Closest.ContactType == KCCContactType.WALL)
          {
            var inputDirection = kcc->Input.Left.IsDown ? -1 : kcc->Input.Right.IsDown ? 1 : 0;
            var oppositeInput = kcc->Closest.Contact.Normal.X * inputDirection < 0;

            if (previousState != KCCState.WALLED)
            {
              kcc->SetStateTimer(f, oppositeInput ? WalledStateExtention : 0);
            }
            if (RequiresOppositeInput == false || oppositeInput)
            {
              kcc->SetStateTimer(f, WalledStateExtention);
            }
          }

          if (previousState != KCCState.WALLED)
          {
            f.Events.Landed(entity, kcc->KinematicHorizontalSpeed, KCCState.WALLED);
            kcc->KinematicHorizontalSpeed = 0;
          }

          break;
        case KCCState.JUMPED:
          // keep the state hanging if pressed
          if (kcc->Input.Jump.IsDown) kcc->SetStateTimer(f, FP._1);
          break;
        case KCCState.DOUBLE_JUMPED:
          // keep the state hanging until bumping
          kcc->SetStateTimer(f, FP._1);
          break;
        case KCCState.GROUNDED:
          if (previousState != KCCState.GROUNDED)
          {
            f.Events.Landed(entity, kcc->KinematicVerticalSpeed, KCCState.GROUNDED);
            kcc->KinematicVerticalSpeed = 0;
          }
          break;
      }

      if (kcc->Closest.ContactType == KCCContactType.CEIL && kcc->KinematicVerticalSpeed > 0)
        kcc->KinematicVerticalSpeed = 0;


    }

    private static bool ShouldForcedSwitch(KCCState currentState, KCCContactType contactType)
    {
      if (contactType == KCCContactType.GROUND && currentState != KCCState.DASHING) return true;
      if (contactType == KCCContactType.WALL) return true;
      if (contactType == KCCContactType.CEIL && currentState != KCCState.DASHING && currentState != KCCState.DOUBLE_JUMPED) return true;
      return false;
    }

    public bool SolverIteration(Frame f, EntityRef entity, Transform2D* transform, KCC2D* KCC, ref FPVector2 position, int iteration)
    {
      bool AppliedCorrection = false;
      // for each contact
      for (int c = 0; c < _contactsCount; c++)
      {
        // compute penetration
        var result = _contacts[c];
        result.ContactType = KCCContactType.NONE;
        result.Overlapping = ComputePenetration(f, position, ref _capsuleShape, ref result.Contact);
        result.SurfaceTangent = FPVector2.Rotate(result.Contact.Normal, -FP.Rad_90);
        result.ContactAngle = FPVector2.Angle(FPVector2.Up, result.Contact.Normal);
        // can be used to modify normals, etc
        f.Signals.OnKCC2DSolverCollision(entity, KCC, &result, iteration);

        _contacts[c] = result;
        if (result.Ignore)
        {
          // continue to next contact (do not apply anything about this)
          continue;
        }

        if (KCC->IgnoreStep)
        {
          _contactsCount = 0;
          return false;
        }

        // identify contact type
        if (result.ContactAngle < MaxSlopeAngle)
        {
          result.ContactType = KCCContactType.GROUND;
        }
        else
        {
          if (result.ContactAngle > 90 + MaxSlopeAngle)
          {
            result.ContactType = KCCContactType.CEIL;
          }
          else
          {
            if (WallJumpEnabled && result.ContactAngle > MinWallAngle && result.ContactAngle < MaxWallAngle)
            {
              result.ContactType = KCCContactType.WALL;
            }
            else
            {
              result.ContactType = KCCContactType.SLOPE;
            }
          }
        }

        // priority for "closest" contact (ground -> wall -> slope -> ceil)
        if (KCC->Closest.ContactType == KCCContactType.NONE || KCC->Closest.ContactType > result.ContactType)
        {
          KCC->Closest = result;
        }

        // apply correction
        if (result.Contact.OverlapPenetration > AllowedPenetration)
        {
          var fullCorrection = result.Contact.Normal * result.Contact.OverlapPenetration;
          if (Debug) Draw.Ray(position, fullCorrection, ColorRGBA.Red);
          var correction = fullCorrection * IterationCorrectionRate;
          position += correction;
          AppliedCorrection = true;
        }
        // applying back to collection/stored
        _contacts[c] = result;
      }
      return AppliedCorrection;
    }

    public void FindContacts(Frame f, EntityRef me, KCC2D* kcc, FPVector2 position)
    {
      Log.Debug($"KCC2D Mask value: {Mask.BitMask}");

      var hits = f.Physics2D.OverlapShape(position, 0, _capsuleShape, Mask, QueryOptions.HitAll);
      int index = 0;
      for (int i = 0; i < hits.Count; i++)
      {
        var hit = hits[i];
        if (hit.IsTrigger)
        {
          f.Signals.OnKCC2DTrigger(me, kcc, hit);
          if (kcc->IgnoreStep)
          {
            _contactsCount = 0;
            return;
          }
          continue;
        }

        if (hit.Entity == me)
        {
          continue;
        }

        var contact = new KCCQueryResult();
        contact.Contact = hit;
        f.Signals.OnKCC2DPreCollision(me, kcc, &contact);
        // add to contacts if not ignored
        if (contact.Ignore == false)
        {
          _contacts[index++] = contact;
        }
        if (kcc->IgnoreStep)
        {
          _contactsCount = 0;
          return;
        }
        if (index >= _contacts.Length) break;
      }
      _contactsCount = index;
    }

    private void IntegrateForces(Frame f, EntityRef entity, Transform2D* transform, KCC2D* KCC)
    {
      var sideMovement = SideMovement(KCC);

      Accelerate(f, KCC, sideMovement);

      // drag
      if (sideMovement == 0)
      {
        if (KCC->State == KCCState.GROUNDED)
        {
          KCC->_kinematicVelocity *= FPMath.Clamp01(1 - Deceleration * f.DeltaTime);
        }
        else
        {
          KCC->KinematicHorizontalSpeed *= FPMath.Clamp01(1 - DecelerationOnAir * f.DeltaTime);
        }
      }

      ApplyGravity(f, KCC);

      ClampVelocity(f, KCC);
    }

    private void ClampVelocity(Frame f, KCC2D* kcc)
    {
      FP maxYSpeed = FreeFallMaxSpeed;
      FP maxXSpeed = MaxBaseSpeed;
      switch (kcc->State)
      {
        case KCCState.WALLED:
          if (WallJumpEnabled && kcc->StateTimer.IsExpiredOrNotValid(f) == false) maxYSpeed = WallMaxSpeed;
          break;
        case KCCState.SLOPED:
          maxYSpeed = SlopeMaxSpeed;
          break;
        case KCCState.DASHING:
          maxXSpeed = MaxDashSpeed;
          break;
      }

      if (kcc->State == KCCState.GROUNDED)
      {
        if (kcc->_kinematicVelocity.Magnitude > maxXSpeed)
        {
          kcc->_kinematicVelocity = kcc->_kinematicVelocity.Normalized * maxXSpeed;
        }
      }
      else if (FPMath.Abs(kcc->KinematicHorizontalSpeed) > maxXSpeed)
      {
        kcc->KinematicHorizontalSpeed = FPMath.Sign(kcc->KinematicHorizontalSpeed) * maxXSpeed;
      }

      if (kcc->KinematicVerticalSpeed < 0)
      {
        kcc->KinematicVerticalSpeed = FPMath.Clamp(kcc->KinematicVerticalSpeed, -maxYSpeed, FP._0);
      }
    }

    private void ApplyGravity(Frame f, KCC2D* KCC)
    {
      bool dashing = KCC->State == KCCState.DASHING;

      if (KCC->State != KCCState.GROUNDED && (DashSuspendsGravity == false || dashing == false))
      {
        FP gravityModifier = 1;
        var buttonPressed = KCC->Input.Jump.IsDown || DownGravityOnRelease == false;
        if (KCC->KinematicVerticalSpeed <= 0 || buttonPressed == false || KCC->State == KCCState.FREE_FALLING) gravityModifier = DownGravityMultiplier;
        KCC->KinematicVerticalSpeed += BaseGravity * gravityModifier * f.DeltaTime;
      }
    }

    private void Accelerate(Frame f, KCC2D* KCC, FP sideMovement)
    {
      if (KCC->State != KCCState.GROUNDED)
      {
        KCC->KinematicHorizontalSpeed += Acceleration * f.DeltaTime * sideMovement;
        if (KCC->State == KCCState.WALLED)
        {
          if (KCC->KinematicHorizontalSpeed * KCC->Closest.Contact.Normal.X < 0) KCC->KinematicHorizontalSpeed = 0;
        }
      }
      else
      {
        KCC->ApplyKinematicAcceleration(f, (Acceleration * sideMovement) * KCC->Closest.SurfaceTangent);
      }
    }

    private FP SideMovement(KCC2D* KCC)
    {
      FP sideMovement = 0;
      if (KCC->Input.Left) sideMovement -= 1;
      if (KCC->Input.Right) sideMovement += 1;
      if (sideMovement != 0) KCC->LastInputDirection = sideMovement.AsInt;
      if (KCC->State != KCCState.GROUNDED) sideMovement *= AirControlFactor;

      var oppositeDirection = KCC->CombinedVelocity.X * sideMovement < 0;
      if (oppositeDirection && (KCC->State == KCCState.GROUNDED || FastFlipOnAir))
      {
        sideMovement *= FlipDirectionMultiplier;
      }

      return sideMovement;
    }

    private static bool ComputePenetration(Frame f, FPVector2 position, ref Shape2D shape, ref Hit hit)
    {

      var t = new Transform2D() { Position = position };
      var s = shape;
      var h = hit;
      var hits = f.Physics2D.CheckOverlap(&s, &t, &h);
      if (hits.Count > 0)
      {
        hit.Normal = hits[0].Normal;
        hit.OverlapPenetration = hits[0].OverlapPenetration;
        return true;
      }
      return false;
    }
  }
}