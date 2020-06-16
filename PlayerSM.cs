using Godot;
using System;
using StateMachine;
using Godot.Collections;

public class PlayerSM : Godot.KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    String[] states;
    FSM machine;
    private static float MovSpeed = 150.0f;
    public float max_speed = 10f;
    private const float ClimbSpeed = 50.0f;
    private static int facing = 1;
    public const float MaxFall = 0.5f;
    //public const float MaxFall = 160f;
    private const float HalfGravThreshold = 40f;
    private static float JumpHeight = 30.0f;
    private static float TimeToApex = 0.30f;
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
    private bool CanHide = false;

    private Timer DashTimer;
    private Timer ClimbTimer;
    private float DashInterval = 1.25f;
    private float prevPosition;
    private float ClimbInterval = 1.25f;
    private bool CanDash = true;
    private bool CanDashJump = true;
    private bool CanClimb = true;
    private string Direction;
    private Vector2 StartingPosition;
    private Vector2 movement = Vector2.Zero;
    private Vector2 up_dir = Vector2.Up;
    private AnimationPlayer animation;
    private Sprite playerSprite;

    private Node2D rightWallRays;
    private Node2D leftWallRays;
    private int WallDirection = 0;
    

    public override void _Ready()
    {
        animation = GetNode<AnimationPlayer>("AnimationPlayer");
        rightWallRays = GetNode<Node2D>("RightWallRays");
        leftWallRays = GetNode<Node2D>("LeftWallRays");

        playerSprite = GetNode<Sprite>("Sprite");
        this.jumping = false;
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

		states = new String[]{"Idle", "Walk", "Jump", "Hide", "Dash", "Crouch", "Climb", "Attack", "WallSlide"};
		machine = new FSM(states,this);
    }

    public void OnEnterIdle(){
        movement.x = 0;
        animation.CurrentAnimation = "Idle";
    }

    public void OnEnterWalk(){
        if(animation.CurrentAnimation != "Walk"){
            animation.CurrentAnimation = "Walk";
        }
        if(Input.IsActionPressed("move_right"))
        {
            movement.x = MovSpeed;
            facing = 1;
            playerSprite.FlipH = false;
        } else if (Input.IsActionPressed("move_left")) {
            movement.x = -MovSpeed;
            facing = -1;
            playerSprite.FlipH = true;
        } else if (Input.IsActionJustPressed("jump") && IsOnFloor()){
            machine.ChangeState("Jump");
        }
    }

    public void OnEnterJump(){
        movement.y = 0;
        movement.y += -(jumpVelocity);
        animation.CurrentAnimation = "Jump";
    }

    public void OnEnterHide(){
        //change sprite or alpha
       // animation.CurrentAnimation = "Hide";
    }

    public void OnEnterDash(){
        movement.x = facing*MovSpeed*DashBoost;
        movement.y += -jumpVelocity/2;
        CanDash = false;
        GD.Print(machine.GetCurrentStateName());
        DashTimer.Start();
      //  animation.CurrentAnimation = "Dash";
    }

    public void OnEnterCrouch(){
        animation.CurrentAnimation = "Crouch";
    }

    public void OnEnterClimb(){
        //animation.CurrentAnimation = "Climb";
    }

    public void OnEnterAttack(){
        //animation.CurrentAnimation = "Attack";
    }

     public void OnUpdateIdle(){
         //if(!IsOnFloor()){
           //  machine.ChangeState("Jump");
         //}
    }

    public void OnUpdateWalk(){
        if(Input.IsActionJustReleased("move_right") || Input.IsActionJustReleased("move_left")){
            machine.ChangeState("Idle");
        } else if (Input.IsActionJustPressed("jump") && IsOnFloor()){
            machine.ChangeState("Jump");
        }
    }

    public void OnUpdateJump(){
        if(!IsOnFloor()){
            if(Math.Abs(movement.y) < JumpHeight){
                animation.CurrentAnimation = "Fall";
            } 
         } else if (IsOnFloor()){
             if(movement.x != 0){
                 machine.ChangeState("Walk");
             } else {
                 machine.ChangeState("Idle");
             }
         }
    }

    public void OnUpdateHide(){
        animation.CurrentAnimation = "Hide";
    }

    public void OnUpdateDash(){
        if(Math.Abs(prevPosition - playerSprite.GlobalPosition.x) >= 100){
            machine.ChangeState("Idle");
        } else if(WallDirection != 0){
            machine.ChangeState("Idle");
        }
    }

    public void OnUpdateCrouch(){
        if(Input.IsActionJustReleased("Crouch")){
            machine.ChangeState("Idle");
        }
    }

    public void OnUpdateClimb(){
        //animation.CurrentAnimation = "Climb";
    }

    public void OnUpdateAttack(){
        animation.CurrentAnimation = "Attack";
        
    }

    public void OnExitIdle(){
    }

    public void OnExitWalk(){
        movement.x = 0;
    }

    public void OnExitJump(){
    }

    public void OnExitHide(){
        //alpha change
    }

    public void OnExitDash(){
    }

    public void OnExitCrouch(){
    }

    public void OnExitClimb(){
        //fall
    }

    public void OnExitAttack(){
    }

    void OnTimeComplete(){
        CanDash = true;
    }

    void OnClimbExpired(){
        CanClimb = false;
    }

    void UpdateWallDirection(){
        bool is_near_left_wall = CheckWallIsValid(leftWallRays);
        bool is_near_right_wall = CheckWallIsValid(rightWallRays);
        if(is_near_right_wall && is_near_left_wall){
            WallDirection = facing;
        } else {
            WallDirection = -Convert.ToInt32(is_near_left_wall) + Convert.ToInt32(is_near_right_wall);
        }
    }
    void HideZoneCheck(){
        
    }
    void UpdateMoveDir(){

    }
    bool CheckWallIsValid(Node2D wallraycast){
        foreach(RayCast2D cast in wallraycast.GetChildren()){
            //var collider = cast.GetCollider();
            //GD.Print(collider);
            //cast is not colliding
            if(cast.IsColliding()){
                GD.Print("HIT");
                var dot = Math.Acos(Vector2.Up.Dot(cast.GetCollisionNormal()));
                if(dot > Math.PI * 0.35 && dot < Math.PI * 0.55){
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }
        return false;
    }

    public override void _PhysicsProcess(float delta)
    {
        UpdateWallDirection();
        PlayerMovement(delta);
    }
    void PlayerMovement(float delta) {
        if(!IsOnFloor()){
            movement.y += GravityFloat * delta;
        } else {
            movement.y = 0;
        }

        /*if(IsOnWall() && CanClimb){
            movement.y = 0f;
            machine.ChangeState("Climb");
        }*/
        //respawn
        if(this.GlobalPosition.y > 0){
            this.GlobalPosition = new Vector2(64, -80);
        };
        
        //reset climb
        if(IsOnFloor()){
            CanClimb = true;
            CanDashJump = true;
        }

        if(Input.IsActionPressed("crouch")){
            machine.ChangeState("Crouch");
        }

        //dash
        if(CanDash && Input.IsActionJustPressed("dash")){
            prevPosition = playerSprite.GlobalPosition.x;
            machine.ChangeState("Dash");
        } 

        //MovementHorz() and jump;
        if((Input.IsActionJustPressed("jump") && IsOnFloor()) || (Input.IsActionJustPressed("jump") && !CanDash && CanDashJump)) {
            CanDashJump = false;
            machine.ChangeState("Jump");
        }

        if(Input.IsActionPressed("move_right") || Input.IsActionPressed("move_left"))
        {  
            if(Input.IsActionJustPressed("jump") && IsOnFloor()){
                machine.ChangeState("Jump");
            } else if (Input.IsActionJustPressed("Dash")) {
                machine.ChangeState("Dash");
            } else {
                machine.ChangeState("Walk");
            }
        } else if (movement.x == 0 && movement.y == 0 && machine.GetCurrentStateName() != "Crouch"){
            machine.ChangeState("Idle");
        }

    if(movement.x != 0 || movement.y != 0){
        //GD.Print("State" + machine.GetCurrentStateName());
        //GD.Print(WallDirection);
        
        MoveAndSlide(movement, up_dir); 
    }
    
        machine.Update();

    }
}
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

