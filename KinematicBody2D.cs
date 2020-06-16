using Godot;
using System;

public class KinematicBody2D : Godot.KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private static float MovSpeed = 150.0f;
    private const float ClimbSpeed = 50.0f;
    private static int facing = 1;
    public const float MaxFall = 0.5f;
    //public const float MaxFall = 160f;
    private const float HalfGravThreshold = 40f;
    private static float JumpHeight = 20.0f;
    private static float TimeToApex = 0.25f;
    private static float GravityFloat = (2*JumpHeight)/(Mathf.Pow(TimeToApex, 2));
    //private static float GravityFloat = 900f;
    private float jumpVelocity = GravityFloat*TimeToApex;
    private static float fallingGrav = 2.0f;
    //private float gravity = 9f;
    private const float JumpGraceTime = 0.1f;
    private static float CoyoteTime = 0.1f;
    private const float JumpSpeed = 105f;
    private const float JumpHBoost = 40f;
    private const float DashBoost = 40f;
    private const float VarJumpTime = .2f;
    private bool jumping;
    private float airborneTime;
    private bool falling;
    private bool hiding;
    private Tween TweenHide;
    private Timer DashTimer;
    private Timer ClimbTimer;
    private float DashInterval = 1.25f;
    private float ClimbInterval = 1.25f;
    private bool CanDash = true;
    private bool CanClimb = true;
    private string Direction;

    private Vector2 StartingPosition;
    private Vector2 movement = Vector2.Zero;
    private Vector2 up_dir = Vector2.Up;
    private AnimationPlayer animation;
    private Sprite playerSprite;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        animation = GetNode<AnimationPlayer>("AnimationPlayer");
        playerSprite = GetNode<Sprite>("Sprite");
        this.jumping = false;
        this.airborneTime = Mathf.Pow(10,20);
        Vector2 StartingPosition = this.Position;
        TweenHide = new Tween();

        DashTimer = new Timer();
        DashTimer.OneShot = true;
        DashTimer.WaitTime = DashInterval;
        DashTimer.Connect("timeout", this, "OnTimeComplete");
        AddChild(DashTimer);
        ClimbTimer = new Timer();
        ClimbTimer.OneShot = true;
        ClimbTimer.WaitTime = ClimbInterval;
        ClimbTimer.Connect("timeout", this, "OnClimbExpired");
        AddChild(ClimbTimer);
        
    }

    void OnTimeComplete(){
        CanDash = true;
    }

    void OnClimbExpired(){
        CanClimb = false;
    }

public override void _Input(InputEvent inputEvent)
{
    if(inputEvent.IsActionPressed("jump")){
        
    }
}

public override void _PhysicsProcess(float delta)
{
    PlayerMovement(delta);
}

public static float Approach(float val, float target, float maxMove){
    return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
}

public static float Lerp(float firstFloat, float secondFloat, float by)
{
     return firstFloat * (1 - by) + secondFloat * by;
}

void PlayerMovement(float delta) {
   if(!IsOnWall() && !IsOnFloor()){
        //float max = MaxFall;
        //float mult = (Math.Abs(movement.y) < HalfGravThreshold) ? .5f : 1f;
        //movement.y += Approach(movement.y, max, GravityFloat * mult * delta);
        //movement.y += gravity/2;
        movement.y += GravityFloat * delta;
    } else if(IsOnWall() && CanClimb){
        movement.y = 0f;
        MoveVertWall(delta);
    } else if(IsOnFloor()){
        CanClimb = true;
    } else if(IsOnWall() && !CanClimb) {
        movement.y += GravityFloat * delta;
    }
    
    MovementHorz();
    Jump();
   
    if(this.GlobalPosition.y > 0){
        this.GlobalPosition = new Vector2(64, -80);
    };
    MoveAndSlide(movement, up_dir);
    
}

void MoveVertWall(float delta){
        ClimbTimer.Start();
        if(Input.IsActionPressed("move_up")){
            movement.y = -ClimbSpeed;
        } else if (Input.IsActionPressed("move_down")) {
            movement.y = ClimbSpeed;
        } else if(Input.IsActionJustPressed("jump")){
            movement.y = -(jumpVelocity);
            movement.x = -facing*MovSpeed;
            AnimateMovement(true, false);
        }  
}

void Jump(){
     if(IsOnFloor()){
        if(Input.IsActionJustPressed("jump")){
            movement.y = 0;
            movement.y += -(jumpVelocity);
            if(facing > 0){
                AnimateMovement(true, true);
            } else {
                AnimateMovement(true, false);
            }
            //movement.y = -Approach(movement.y, jumpVelocity, JumpHeight);
            //movement.x = facing*Approach(movement.x, MovSpeed, MovSpeed * 5);
        } 
    }  else if (!CanDash) {
        if(Input.IsActionJustPressed("jump")){
            movement.y = 0;
            movement.y += -(jumpVelocity);
            if(facing > 0){
                AnimateMovement(true, true);
            } else {
                AnimateMovement(true, false);
            }
            //movement.y = -Approach(movement.y, jumpVelocity, JumpHeight);
            //movement.x = facing*Approach(movement.x, MovSpeed, MovSpeed * 5);
        } 
    }
}

void Dash(){
    //movement.x = facing*Lerp(movement.x, movement.x+MovSpeed*DashBoost, (float)0.2);
    movement.x = facing*MovSpeed*DashBoost;
    movement.y += -jumpVelocity;
    CanDash = false;
    DashTimer.Start();
}

void Stealth(){
    if(Input.IsActionJustPressed("hide")){
        
    }
}

void MovementHorz(){
    if(!IsOnWall() && IsOnFloor()){
        if(Input.IsActionPressed("move_right"))
        {
            movement.x = MovSpeed;
            facing = 1;
            AnimateMovement(true, true);
        } else if (Input.IsActionPressed("move_left")) {
            movement.x = -MovSpeed;
            facing = -1;
            AnimateMovement(true, false);
        } else if(Input.IsActionJustPressed("dash") && CanDash){
            Dash();
        } else {
            movement.x = 0.0f;
            AnimateMovement(false, false);
        }
    } else if (!IsOnWall()){
        if(Input.IsActionPressed("move_right"))
        {
            movement.x = MovSpeed/2;
            facing = 1;
            AnimateMovement(true, true);
        } else if (Input.IsActionPressed("move_left")) {
            movement.x = -MovSpeed/2;
            facing = -1;
            AnimateMovement(true, false);
        } else if (Input.IsActionJustPressed("dash") && CanDash){
            Dash();
        } else {
            movement.x = 0.0f;
            AnimateMovement(false, false);
        }
    }
}

void AnimateMovement(bool moving, bool movingRight){
    if(moving && IsOnFloor()){
        animation.Play("Walk");
        if(movingRight){
            playerSprite.FlipH = false;
        } else {
            playerSprite.FlipH = true;
        }
    } else if(!IsOnFloor() && moving){
        if(movement.y < JumpHeight){
            animation.Play("Jump");
        } else {
            animation.Play("Fall");
        }
    } else {
        animation.Play("Idle");
    }
}



//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
