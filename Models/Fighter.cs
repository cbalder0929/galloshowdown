using GalloShowdown.Combat;
using GalloShowdown.Engine;
using GalloShowdown.Input;

namespace GalloShowdown.Models;

public enum CurrentAttackType { None, Light, Heavy }

public abstract class Fighter
{
    public const double BodyWidth  = 120;
    public const double BodyHeight = 200;
    public const double CrouchHeight = 140;

    public string Name { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int Attack { get; }
    public int Defense { get; }
    public double Speed { get; }

    public double X { get; private set; }
    public double Y { get; private set; }
    public int Facing { get; private set; } = 1;
    public FighterState State { get; private set; }

    private double _vx, _vy;
    private int _frameCounter;
    private Move? _currentMove;
    private int _hitStunFrames;

    // Prevents the same attack swing from registering multiple hits.
    public bool HasAlreadyHit { get; private set; }

    public CurrentAttackType CurrentAttack { get; private set; }

    protected Fighter(string name, int maxHp, int atk, int def, double speed)
    {
        Name = name;
        MaxHealth = maxHp;
        Health = maxHp;
        Attack = atk;
        Defense = def;
        Speed = speed;
    }

    public abstract Move LightMove { get; }
    public abstract Move HeavyMove { get; }

    public virtual void Update(double dt, InputCommand cmd, double arenaWidth)
    {
        const double Gravity = 1800;
        const double JumpVelocity = 700;

        if (State == FighterState.KnockedOut) return;

        bool grounded = Y <= 0;

        if (State == FighterState.HitStun)
        {
            _hitStunFrames--;
            if (_hitStunFrames <= 0)
                State = grounded ? FighterState.Idle : FighterState.Jumping;
            _vx *= Math.Pow(0.001, dt);
            ApplyPhysics(dt, arenaWidth, Gravity);
            return;
        }

        if (State == FighterState.Attacking)
        {
            _frameCounter++;
            if (_frameCounter >= _currentMove!.TotalFrames)
            {
                State = grounded ? FighterState.Idle : FighterState.Jumping;
                _currentMove  = null;
                CurrentAttack = CurrentAttackType.None;
                HasAlreadyHit = false;
            }
            ApplyPhysics(dt, arenaWidth, Gravity);
            return;
        }

        bool lightAtk  = cmd.HasFlag(InputCommand.LightAttack);
        bool heavyAtk  = cmd.HasFlag(InputCommand.HeavyAttack);
        bool crouch    = cmd.HasFlag(InputCommand.Crouch);
        bool jump      = cmd.HasFlag(InputCommand.Jump);
        bool moveLeft  = cmd.HasFlag(InputCommand.MoveLeft);
        bool moveRight = cmd.HasFlag(InputCommand.MoveRight);

        if ((lightAtk || heavyAtk) &&
            (State == FighterState.Idle || State == FighterState.Walking || State == FighterState.Jumping))
        {
            _currentMove  = lightAtk ? LightMove : HeavyMove;
            CurrentAttack = lightAtk ? CurrentAttackType.Light : CurrentAttackType.Heavy;
            State = FighterState.Attacking;
            _frameCounter = 0;
            _vx = 0;
            HasAlreadyHit = false;
            ApplyPhysics(dt, arenaWidth, Gravity);
            return;
        }

        if (moveLeft)       _vx = -Speed;
        else if (moveRight) _vx =  Speed;
        else if (grounded)  _vx = 0;

        if (crouch && grounded)
        {
            State = FighterState.Crouching;
            _vx = 0;
        }
        else if (jump && grounded)
        {
            _vy = JumpVelocity;
            State = FighterState.Jumping;
        }
        else if (grounded)
        {
            State = (moveLeft || moveRight) ? FighterState.Walking : FighterState.Idle;
        }

        if (!grounded && State != FighterState.Jumping)
            State = FighterState.Jumping;

        ApplyPhysics(dt, arenaWidth, Gravity);
    }

    private void ApplyPhysics(double dt, double arenaWidth, double gravity)
    {
        _vy -= gravity * dt;
        Y   += _vy * dt;
        X   += _vx * dt;

        if (Y <= 0)
        {
            Y   = 0;
            _vy = 0;
            if (State == FighterState.Jumping) State = FighterState.Idle;
        }

        X = Math.Clamp(X, 0, arenaWidth - BodyWidth);
    }

    public virtual int TakeDamage(int rawDamage, bool blocking)
    {
        int reduced = Math.Max(1, rawDamage - Defense);
        if (blocking) reduced /= 2;
        Health = Math.Max(0, Health - reduced);
        if (Health == 0) State = FighterState.KnockedOut;
        return reduced;
    }

    public void EnterHitStun(int frames, double knockbackVx)
    {
        if (State == FighterState.KnockedOut) return;
        State = FighterState.HitStun;
        _hitStunFrames = frames;
        _vx = knockbackVx;
    }

    public void MarkHitLanded() => HasAlreadyHit = true;

    public Hitbox? GetActiveHitbox()
    {
        if (_currentMove == null || State != FighterState.Attacking) return null;
        if (_frameCounter <  _currentMove.StartupFrames) return null;
        if (_frameCounter >= _currentMove.StartupFrames + _currentMove.ActiveFrames) return null;
        return _currentMove.BuildHitbox(X, Y, Facing);
    }

    public int GetActiveMoveDamage()      => _currentMove?.Damage ?? 0;
    public int GetActiveMoveHitStunFrames() => _currentMove != null ? _currentMove.ActiveFrames + 4 : 0;

    public bool IsInActiveFrames =>
        _currentMove != null && State == FighterState.Attacking &&
        _frameCounter >= _currentMove.StartupFrames &&
        _frameCounter <  _currentMove.StartupFrames + _currentMove.ActiveFrames;

    public bool IsInStartupFrames =>
        _currentMove != null && State == FighterState.Attacking &&
        _frameCounter < _currentMove.StartupFrames;

    public Hitbox GetHurtbox()
    {
        double h = State == FighterState.Crouching ? CrouchHeight : BodyHeight;
        return new Hitbox(X, Y, BodyWidth, h);
    }

    public void FaceTowards(double otherX)
    {
        if (State == FighterState.KnockedOut) return;
        Facing = otherX > X ? 1 : -1;
    }

    public void PlaceAt(double x, double y)
    {
        X = x;
        Y = y;
    }
}
