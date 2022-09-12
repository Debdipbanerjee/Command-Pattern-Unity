using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    private Invoker invoker;

    public interface ICommand
    {
        void Execute();
        void ExecuteUndo();
    }

    // Command for basic movement (arrow keys)
    public class CommandMove : ICommand
    {
        GameObject GameObject;
        Vector3 Direction;

        public CommandMove(GameObject obj, Vector3 direction)
        {
            GameObject = obj;
            Direction = direction;
        }
        public void Execute()
        {
            GameObject.transform.position += Direction;
        }
        public void ExecuteUndo()
        {
            GameObject.transform.position -= Direction;
        }
    }

    // Command for Point Click Movement (Mouse)
    public class CommandMoveTo : ICommand
    {
        GameManager GameManager;
        Vector3 Destination;
        Vector3 StartPosition;

        public CommandMoveTo(GameManager manager, Vector3 startPos, Vector3 destPos)
        {
            GameManager = manager;
            Destination = destPos;
            StartPosition = startPos;
        }
        public void Execute()
        {
            GameManager.MoveTo(Destination);
        }
        public void ExecuteUndo()
        {
            GameManager.MoveTo(StartPosition);
        }
    }

    public class Invoker
    {
        public Invoker()
        {
            Commands = new Stack<ICommand>();
        }
        public void Execute(ICommand command)
        {
            if (command != null)
            {
                Commands.Push(command);
                Commands.Peek().Execute();
            }
        }
        public void Undo()
        {
            if (Commands.Count > 0)
            {
                Commands.Peek().ExecuteUndo();
                Commands.Pop();
            }
        }
        Stack<ICommand> Commands;
    }

    public Vector3? GetClickPosition()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo))
            {
                return hitInfo.point;
            }
        }
        return null;
    }

    public IEnumerator MoveToInSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        end.y = startingPos.y;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        objectToMove.transform.position = end;
    }

    // Start is called before the first frame update
    void Start()
    {
        invoker = new Invoker();
    }

    // Update is called once per frame
    void Update()
    {
        // Arrowkeys
        Vector3 dir = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            dir.z = 1.0f;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            dir.z = -1.0f;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            dir.x = -1.0f;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            dir.x = 1.0f;
        if (dir != Vector3.zero)
        {
            //player.transform.position += dir;
            CommandMove move = new CommandMove(player, dir);
            invoker.Execute(move);
        }

        // Click to point
        var clickPoint = GetClickPosition();
        if (clickPoint != null)
        {
            CommandMoveTo moveto = new CommandMoveTo(this, player.transform.position, clickPoint.Value);
            invoker.Execute(moveto);
        }

        // Undo
        if (Input.GetKeyDown(KeyCode.Z))
        {
            invoker.Undo();
        }
    }

    public void MoveTo(Vector3 pt)
    {
        IEnumerator moveto = MoveToInSeconds(player, pt, 0.5f);
        StartCoroutine(moveto);
    }
}
