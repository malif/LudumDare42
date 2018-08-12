using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FaceDir
{
    UP, RIGHT, DOWN, LEFT
};

public class Player_Forklift : MonoBehaviour
{
    public float MoveTime = 1.0f;
    public float LiftSpeed = 5.0f;
    public float RotateSpeed = 90.0f;


    private bool MoveInProgress = false;
    private bool TurnInProgress = false;
    private Vector3 EndPos;
    private Quaternion EndRot;
    private float MovedDistance = 0.0f;
    private float TurnedDegrees = 0.0f;
    private float RotateDir = 0.0f;
    private bool ForwardMove = false;

    private bool LiftMoveInProgress = false;

    public GameObject Lift = null;

    public FaceDir FacingDir = FaceDir.LEFT;

    private GameObject BottomPallet = null;
    private int CurrentLiftLevel = 0;

    public GameObject DebugText = null;

    // Use this for initialization
    void Start()
    {

    }

    ////////////////////////////////////////////////////////
    /// UPDATE
    ////////////////////////////////////////////////////////
    void Update()
    {
        if (MoveInProgress)
        {
            UpdateMoveInProgress();
        }
        else if (TurnInProgress)
        {
            DebugText.GetComponent<Text>().text = "Turning";
            UpdateTurnInProgress();
        }
        else if (LiftMoveInProgress)
        {
            DebugText.GetComponent<Text>().text = "Lifting";
            UpdateLiftInProgress();
        }
        else
        {
            DebugText.GetComponent<Text>().text = "None";
        }
        if(GameLogic.Instance.InputAllowed())
            UpdateInput();
    }

    ////////////////////////////////////////////////////////
    /// HELPERS
    ////////////////////////////////////////////////////////

    FaceDir GetRightDir()
    {
        switch (FacingDir)
        {
            case FaceDir.UP:
                return FaceDir.RIGHT;
            case FaceDir.LEFT:
                return FaceDir.UP;
            case FaceDir.RIGHT:
                return FaceDir.DOWN;
            case FaceDir.DOWN:
                return FaceDir.LEFT;
        }
        return FaceDir.RIGHT;
    }

    Vector3 GetMoveDir(FaceDir direction)
    {
        switch (direction)
        {
            case FaceDir.UP:
                return new Vector3(0.0f, 0.0f, 1.0f);
            case FaceDir.LEFT:
                return new Vector3(-1.0f, 0.0f, 0.0f);
            case FaceDir.RIGHT:
                return new Vector3(1.0f, 0.0f, 0.0f);
            case FaceDir.DOWN:
                return new Vector3(0.0f, 0.0f, -1.0f);
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    private bool CheckAllCollisions(Vector3 Position)
    {
        Collider[] collisions = Physics.OverlapBox(Position, new Vector3(0.4f, 0.4f, 0.4f));
        foreach (Collider col in collisions)
        {
            if (col.gameObject.GetComponent<Pallet>() != null)
                return false;

            if (col.gameObject.GetComponent<Blocker>() != null)
                return false;
        }
        return true;
    }

    ////////////////////////////////////////////////////////
    /// MOVEMENT MANAGEMENT
    ////////////////////////////////////////////////////////
    void UpdateMoveInProgress()
    {
        if (!MoveInProgress)
            return;

        Vector3 moveDir = EndPos - transform.position;
        moveDir.Normalize();

        Vector3 MoveDelta = moveDir * Mathf.Clamp(MoveTime * GameLogic.Instance.DeltaTime(), 0.0f, 1.0f - MovedDistance);
        transform.position += MoveDelta;

        MovedDistance += MoveDelta.magnitude;

        if (BottomPallet != null)
            UpdatePalletPosition(MoveDelta);

        DebugText.GetComponent<Text>().text = "Moved Distance: " + MovedDistance;
        if (MovedDistance >= 0.99f)
        {
            transform.position = EndPos;
            MoveInProgress = false;
            MovedDistance = 0.0f;
            if (ForwardMove)
            {
                OnFinalizeMoveForward();
                ForwardMove = false;
            }
            return;
        }
    }

    private void UpdatePalletPosition(Vector3 deltaPos)
    {
        BottomPallet.transform.position += deltaPos;
        BottomPallet.GetComponent<Pallet>().OnPalletPositionUpdated(deltaPos, true);
    }

    void UpdateTurnInProgress()
    {
        if (!TurnInProgress)
            return;

        float rotateAmount = Mathf.Clamp(RotateDir * RotateSpeed * GameLogic.Instance.DeltaTime(), -90.0f - TurnedDegrees, 90.0f - TurnedDegrees);
        TurnedDegrees += rotateAmount;
        transform.Rotate(new Vector3(0.0f, rotateAmount, 0.0f));

        if (BottomPallet != null)
            UpdatePalletRotation(rotateAmount);

        if (Mathf.Abs(TurnedDegrees) >= 89.99f)
        {
            transform.rotation = EndRot;
            TurnInProgress = false;
            TurnedDegrees = 0.0f;
            return;
        }
    }

    private void UpdatePalletRotation(float degrees)
    {
        BottomPallet.transform.RotateAround(transform.position, new Vector3(0.0f, 1.0f, 0.0f), degrees);
        BottomPallet.GetComponent<Pallet>().OnPalletRotationUpdated(degrees, transform.position, true);
    }

    public bool CanMoveForwards()
    {
        if(CurrentLiftLevel == 0)
        {
            Collider[] collisions = Physics.OverlapBox(transform.position + GetMoveDir(FacingDir) * 2, new Vector3(0.4f, 0.4f, 0.4f));
            foreach (Collider col in collisions)
            {
                if (col.gameObject.GetComponent<Pallet>() != null)
                {
                    if (BottomPallet != null)
                        return false;
                }

                if (col.gameObject.GetComponent<Blocker>() != null)
                    return false;
            }
        }
        else
        {
            Collider[] collisions = Physics.OverlapBox(transform.position + GetMoveDir(FacingDir) * 2 + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.4f, 0.4f, 0.4f));
            foreach (Collider col in collisions)
            {
                if (col.gameObject.GetComponent<Pallet>() != null)
                {
                    if (BottomPallet != null)
                        return false;
                }

                if (col.gameObject.GetComponent<Blocker>() != null)
                    return false;
            }

            Collider[] lowCollisions = Physics.OverlapBox(transform.position + GetMoveDir(FacingDir), new Vector3(0.4f, 0.4f, 0.4f));
            foreach (Collider col in lowCollisions)
            {
                if (col.gameObject.GetComponent<Pallet>() != null)
                {
                    return false;
                }

                if (col.gameObject.GetComponent<Blocker>() != null)
                    return false;
            }
        }
        
        return true;
    }

    public bool CanMoveBackwards()
    {
        return CheckAllCollisions(transform.position - GetMoveDir(FacingDir));
    }

    public bool CanRotate(float rotateDir)
    {
        Vector3 sidePos = rotateDir * GetMoveDir(GetRightDir());
        Vector3 forwardPos = GetMoveDir(FacingDir);
        bool sideCheck = CheckAllCollisions(transform.position + sidePos + new Vector3(0.0f, CurrentLiftLevel, 0.0f));
        bool upperSideCheck = CheckAllCollisions(transform.position + new Vector3(0.0f, CurrentLiftLevel, 0.0f) + forwardPos + sidePos);

        return sideCheck && upperSideCheck;
    }

    void MoveForward()
    {
        if (!CanMoveForwards())
            return;
        Vector3 moveDelta = GetMoveDir(FacingDir);

        EndPos = transform.position + moveDelta;
        ForwardMove = true;
        MoveInProgress = true;
    }

    void MoveBackward()
    {
        if (!CanMoveBackwards())
            return;
        Vector3 moveDelta = -GetMoveDir(FacingDir);
        EndPos = transform.position + moveDelta;
        MoveInProgress = true;
        OnStartMoveBackward();
    }

    void TurnClockwise()
    {
        if (!CanRotate(1.0f))
            return;

        switch (FacingDir)
        {
            case FaceDir.UP:
                FacingDir = FaceDir.RIGHT;
                break;
            case FaceDir.LEFT:
                FacingDir = FaceDir.UP;
                break;
            case FaceDir.RIGHT:
                FacingDir = FaceDir.DOWN;
                break;
            case FaceDir.DOWN:
                FacingDir = FaceDir.LEFT;
                break;
            default:
                {
                    // Do nothing
                }
                break;
        }

        transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
        EndRot = transform.rotation;
        transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
        RotateDir = 1.0f;
        TurnInProgress = true;
    }

    void TurnAntiClockwise()
    {
        if (!CanRotate(-1.0f))
            return;

        switch (FacingDir)
        {
            case FaceDir.UP:
                FacingDir = FaceDir.LEFT;
                break;
            case FaceDir.LEFT:
                FacingDir = FaceDir.DOWN;
                break;
            case FaceDir.RIGHT:
                FacingDir = FaceDir.UP;
                break;
            case FaceDir.DOWN:
                FacingDir = FaceDir.RIGHT;
                break;
            default:
                {
                    // Do nothing
                }
                break;
        }

        transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
        EndRot = transform.rotation;
        transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
        RotateDir = -1.0f;
        TurnInProgress = true;
    }

    void UpdateInput()
    {
        if (MoveInProgress || TurnInProgress || LiftMoveInProgress)
            return;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            switch (FacingDir)
            {
                case FaceDir.UP:
                    MoveForward();
                    break;
                case FaceDir.LEFT:
                    TurnClockwise();
                    break;
                case FaceDir.RIGHT:
                    TurnAntiClockwise();
                    break;
                case FaceDir.DOWN:
                    MoveBackward();
                    break;
                default:
                    {
                        // Do nothing
                    }
                    break;
            }
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            switch (FacingDir)
            {
                case FaceDir.UP:
                    TurnAntiClockwise();
                    break;
                case FaceDir.LEFT:
                    MoveForward();
                    break;
                case FaceDir.RIGHT:
                    MoveBackward();
                    break;
                case FaceDir.DOWN:
                    TurnClockwise();
                    break;
                default:
                    {
                        // Do nothing
                    }
                    break;
            }
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            switch (FacingDir)
            {
                case FaceDir.UP:
                    MoveBackward();
                    break;
                case FaceDir.LEFT:
                    TurnAntiClockwise();
                    break;
                case FaceDir.RIGHT:
                    TurnClockwise();
                    break;
                case FaceDir.DOWN:
                    MoveForward();
                    break;
                default:
                    {
                        // Do nothing
                    }
                    break;
            }
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            switch (FacingDir)
            {
                case FaceDir.UP:
                    TurnClockwise();
                    break;
                case FaceDir.LEFT:
                    MoveBackward();
                    break;
                case FaceDir.RIGHT:
                    MoveForward();
                    break;
                case FaceDir.DOWN:
                    TurnAntiClockwise();
                    break;
                default:
                    {
                        // Do nothing
                    }
                    break;
            }
        }
        else if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.Space))
        {
            if (CurrentLiftLevel == 0 && CanLift())
                RaiseLift();
            else if(CanLower())
                LowerLift();
        }
    }

    void OnFinalizeMoveForward()
    {
        Collider[] collisions = Physics.OverlapBox(transform.position + GetMoveDir(FacingDir) + new Vector3(0.0f, CurrentLiftLevel, 0.0f), new Vector3(0.4f, 0.4f, 0.4f));
        foreach (Collider col in collisions)
        {
            if (col.gameObject.GetComponent<Pallet>() != null)
            {
                AttachPallet(col.gameObject);
                return;
            }
        }
    }

    void OnStartMoveBackward()
    {
        if (BottomPallet == null)
            return;

        if (CurrentLiftLevel == 0)
        {
            DetatchPallet();
        }
        else if(CurrentLiftLevel > 0)
        {
            Collider[] collisions = Physics.OverlapBox(Lift.transform.position - new Vector3(0.0f, CurrentLiftLevel, 0.0f), new Vector3(0.4f, 0.4f, 0.4f));
            foreach (Collider col in collisions)
            {
                Pallet p = col.gameObject.GetComponent<Pallet>();
                if (p!= null)
                {
                    PalletContent pc = p.Contents.GetComponentInChildren<PalletContent>();
                    if (pc != null && pc.Stackable)
                    {
                        DetatchPallet();
                        return;
                    }
                    
                }
            }
        }
    }



    ////////////////////////////////////////////////////////
    /// PALLET MANAGEMENT
    ////////////////////////////////////////////////////////

    private void AttachPallet(GameObject _pallet)
    {
        if (_pallet.GetComponent<Pallet>() == null)
            return;

        if (CurrentLiftLevel > 0)
        {
            Collider[] collisions = Physics.OverlapBox(Lift.transform.position - new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.4f, 0.4f, 0.4f));
            foreach (Collider col in collisions)
            {
                if (col.gameObject.GetComponent<Pallet>() != null)
                {
                    col.gameObject.GetComponent<Pallet>().DetachPallet();
                }
            }
        }

        BottomPallet = _pallet;
        BottomPallet.gameObject.GetComponent<Pallet>().AttachedToForklift = true;
        BottomPallet.gameObject.GetComponent<Pallet>().forklift = this;
    }

    public void DetatchPallet()
    {
        if (BottomPallet == null)
            return;

        BottomPallet.GetComponent<Pallet>().OnDetatched();
        BottomPallet.gameObject.GetComponent<Pallet>().AttachedToForklift = false;
        BottomPallet.gameObject.GetComponent<Pallet>().forklift = null;
        BottomPallet = null;
    }

    ////////////////////////////////////////////////////////
    /// LIFT MANAGEMENT
    ////////////////////////////////////////////////////////
    
    private bool CanLift()
    {
        if (BottomPallet == null)
            return true;

        Pallet p = BottomPallet.GetComponent<Pallet>();
        if (p.StackedPallet != null)
            return false;

        return true;
    }

    private bool CanLower()
    {
        return CheckAllCollisions(Lift.transform.position - new Vector3(0.0f, 1.0f, 0.0f));
    }

    private void RaiseLift()
    {
        if (CurrentLiftLevel != 0)
            return;

        ++CurrentLiftLevel;
        EndPos = Lift.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
        LiftMoveInProgress = true;
    }

    private void LowerLift()
    {
        if (CurrentLiftLevel != 1)
            return;

        --CurrentLiftLevel;
        EndPos = Lift.transform.position + new Vector3(0.0f, -1.0f, 0.0f);
        LiftMoveInProgress = true;
    }

    private void UpdateLiftInProgress()
    {
        if (!LiftMoveInProgress)
            return;

        Vector3 moveDir = EndPos - Lift.transform.position;
        moveDir.Normalize();

        Vector3 MoveDelta = moveDir * Mathf.Clamp(LiftSpeed * GameLogic.Instance.DeltaTime(), 0.0f, 1.0f - MovedDistance);
        Lift.transform.position += MoveDelta;

        MovedDistance += MoveDelta.magnitude;

        if (BottomPallet != null)
            UpdatePalletLift(MoveDelta);

        if (MovedDistance >= 0.99f)
        {
            Lift.transform.position = EndPos;
            LiftMoveInProgress = false;
            MovedDistance = 0.0f;
            return;
        }
    }

    private void UpdatePalletLift(Vector3 deltaPos)
    {
        BottomPallet.transform.position += deltaPos;
        BottomPallet.GetComponent<Pallet>().OnPalletLiftUpdated(deltaPos, true);
    }
}