using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RND = UnityEngine.Random;

public class qkGnomishPuzzleScript : MonoBehaviour {

    public Material[] Symbols;
    public GameObject[] SymbolOBJs;
    public GameObject[] LEDOBJs;

    public GameObject judgeText;

    public GameObject[] Levers;

    public Material Lit;
    public Material unLit;

    private string[] SymbolNames = new string[] { "Shovel", "Fence", "Flamingo", "Flower", "Hand", "Fork", "Boot" };
    private string[] finalOrder = new string[] { "Shovel", "Fence", "Flamingo", "Flower", "Hand", "Fork", "Boot" };
    private Dictionary<GameObject, bool> objectDict = new Dictionary<GameObject, bool>();

    private Dictionary<string, Material> materialDict = new Dictionary<string, Material>();

    private static readonly int moveCount = 45;
    private bool judge = false;
    private bool first = true;
    private bool solved = false;
    private bool ableToInteract = true;
    private List<string> logText = new List<string>();

    private bool _forcesolve = false;

    int moduleId;
    static int moduleIdCounter;

    void Start()
    {
        moduleId = ++moduleIdCounter;
        Transform sym = transform.Find("Objects").Find("TopRow").Find("Symbols");
        Symbols = new Material[] { sym.Find("Shovel").GetComponent<Renderer>().material, sym.Find("Fence").GetComponent<Renderer>().material, sym.Find("Flamingo").GetComponent<Renderer>().material, sym.Find("Flower").GetComponent<Renderer>().material, sym.Find("Hand").GetComponent<Renderer>().material, sym.Find("Fork").GetComponent<Renderer>().material, sym.Find("Boot").GetComponent<Renderer>().material };
        SymbolOBJs = new GameObject[] { sym.Find("Shovel").gameObject, sym.Find("Fence").gameObject, sym.Find("Flamingo").gameObject, sym.Find("Flower").gameObject, sym.Find("Hand").gameObject, sym.Find("Fork").gameObject, sym.Find("Boot").gameObject };
        Transform led = transform.Find("Objects").Find("BottomRow").Find("LEDs");
        LEDOBJs = new GameObject[] { led.Find("1").gameObject, led.Find("2").gameObject, led.Find("3").gameObject, led.Find("4").gameObject, led.Find("5").gameObject, led.Find("6").gameObject, led.Find("7").gameObject };
        objectDict = new Dictionary<GameObject, bool>()
        {
            {SymbolOBJs[0], false},
            {SymbolOBJs[1], false},
            {SymbolOBJs[2], false},
            {SymbolOBJs[3], false},
            {SymbolOBJs[4], false},
            {SymbolOBJs[5], false},
            {SymbolOBJs[6], false},
        };
        materialDict = new Dictionary<string, Material>()
        {
            {"Shovel", Symbols[0]},
            {"Fence", Symbols[1]},
            {"Flamingo", Symbols[2]},
            {"Flower", Symbols[3]},
            {"Hand", Symbols[4]},
            {"Fork", Symbols[5]},
            {"Boot", Symbols[6]},
        };
        switch (RND.Range(1, 6))
        {
            case 1:
                judge = true;
                break;
            default:
                judge = false;
                finalOrder.Shuffle();
                break;
        }
        SymbolNames = finalOrder;
        do { logText.Clear(); for (int i = 0; i < moveCount; i++) { move(); } } while (getSolve());
        for (int i = 0; i < SymbolNames.Length; i++)
        {
            SymbolOBJs[i].GetComponent<Renderer>().material = materialDict[SymbolNames[i]];
            if (SymbolNames[i] == finalOrder[i]) { LEDOBJs[i].GetComponent<Renderer>().material = Lit; }
            else { LEDOBJs[i].GetComponent<Renderer>().material = unLit; }
        }
        Levers[0].GetComponent<KMSelectable>().OnInteract += () => startInteraction(1, Levers[0]);
        Levers[1].GetComponent<KMSelectable>().OnInteract += () => startInteraction(2, Levers[1]);
        Levers[2].GetComponent<KMSelectable>().OnInteract += () => startInteraction(3, Levers[2]);
        Debug.LogFormat("[Gnomish Puzzle #{0}] Starting order is: {1} {2} {3} {4} {5} {6} {7}", moduleId, SymbolNames[0], SymbolNames[1], SymbolNames[2], SymbolNames[3], SymbolNames[4], SymbolNames[5], SymbolNames[6]);
        Debug.LogFormat("[Gnomish Puzzle #{0}] Final order is: {1} {2} {3} {4} {5} {6} {7}", moduleId, finalOrder[0], finalOrder[1], finalOrder[2], finalOrder[3], finalOrder[4], finalOrder[5], finalOrder[6]);
        Debug.LogFormat("[Gnomish Puzzle #{0}] You are {1}worthy", moduleId, judge ? "" : "not ");
        FinalizeLog();
        logText.Reverse();
        Debug.LogFormat("[Gnomish Puzzle #{0}] One possible solution: {1}", moduleId, String.Join(", ", logText.ToArray()));
    }

    private void FinalizeLog()
    {
        string currentStr = "";
        int ind = 0;
        List<int> indexes = new List<int>();
        bool _remove = false;
        for (int i = 0; i < logText.Count; i++)
        {
            string str = logText[i];
            if (str != currentStr)
            {
                currentStr = str;
                ind = 1;
                indexes.Clear();
                indexes.Add(i);
            }
            else
            {
                indexes.Add(i - ind++);
                if (ind == (currentStr == "Middle" ? 2 : 7))
                {
                    _remove = true;
                    break;
                }
            }
        }
        if (_remove)
        {
            foreach (int index in indexes)
            {
                logText.RemoveAt(index);
            }
            FinalizeLog();
        }
    }

    private void move()
    {
        switch (RND.Range(1, 4))
        {
            case 1:
                //Debug.Log("Moving left");
                SymbolNames = new string[] { SymbolNames[1], SymbolNames[2], SymbolNames[3], SymbolNames[4], SymbolNames[5], SymbolNames[6], SymbolNames[0] };
                if (logText.Count > 0 && logText[logText.Count - 1] == "Left") { logText.RemoveAt(logText.Count - 1); } else { logText.Add("Right"); }
                //logText.Add("Right");
                break;
            case 2:
                //Debug.Log("Now middle");
                SymbolNames = new string[] { SymbolNames[0], SymbolNames[1], SymbolNames[4], SymbolNames[3], SymbolNames[2], SymbolNames[5], SymbolNames[6] };
                if (logText.Count > 0 && logText[logText.Count - 1] == "Middle") { logText.RemoveAt(logText.Count - 1); } else { logText.Add("Middle"); }
                //logText.Add("Middle");
                break;
            case 3:
                //Debug.Log("Moving right");
                SymbolNames = new string[] { SymbolNames[6], SymbolNames[0], SymbolNames[1], SymbolNames[2], SymbolNames[3], SymbolNames[4], SymbolNames[5] };
                if (logText.Count > 0 && logText[logText.Count - 1] == "Right") { logText.RemoveAt(logText.Count - 1); } else { logText.Add("Left"); }
                //logText.Add("Left");
                break;
        }
        return;
    }
    private bool getSolve() { if (SymbolNames[0] == finalOrder[0] && SymbolNames[1] == finalOrder[1] && SymbolNames[2] == finalOrder[2] && SymbolNames[3] == finalOrder[3] && SymbolNames[4] == finalOrder[4] && SymbolNames[5] == finalOrder[5] && SymbolNames[6] == finalOrder[6]) { return true; } else { return false; } }

    bool startInteraction(int num, GameObject Lever)
    {
        StartCoroutine(Interact(num, Lever));
        return false;
    }
    IEnumerator Interact(int num, GameObject Lever, bool forceSolve = false)
    {
        yield return null;
        if (!solved && ableToInteract)
        {
            ableToInteract = false;
            List<GameObject> objectsToInteract = new List<GameObject>();
            switch (num)
            {
                case 1:
                    SymbolNames = new string[] { SymbolNames[1], SymbolNames[2], SymbolNames[3], SymbolNames[4], SymbolNames[5], SymbolNames[6], SymbolNames[0] };
                    objectsToInteract = new List<GameObject>() { SymbolOBJs[0], SymbolOBJs[1], SymbolOBJs[2], SymbolOBJs[3], SymbolOBJs[4], SymbolOBJs[5], SymbolOBJs[6] };
                    break;
                case 2:
                    SymbolNames = new string[] { SymbolNames[0], SymbolNames[1], SymbolNames[4], SymbolNames[3], SymbolNames[2], SymbolNames[5], SymbolNames[6] };
                    objectsToInteract = new List<GameObject>() { SymbolOBJs[2], SymbolOBJs[4] };
                    break;
                case 3:
                    SymbolNames = new string[] { SymbolNames[6], SymbolNames[0], SymbolNames[1], SymbolNames[2], SymbolNames[3], SymbolNames[4], SymbolNames[5] };
                    objectsToInteract = new List<GameObject>() { SymbolOBJs[0], SymbolOBJs[1], SymbolOBJs[2], SymbolOBJs[3], SymbolOBJs[4], SymbolOBJs[5], SymbolOBJs[6] };
                    break;
            }
            if (!forceSolve && logText.Count > 0)
            {
                switch (num)
                {
                    case 1:
                        if (logText[0] == "Left") logText.RemoveAt(0);
                        else { logText.Insert(0, "Right"); }
                        break;
                    case 2:
                        if (logText[0] == "Middle") logText.RemoveAt(0);
                        else { logText.Insert(0, "Middle"); }
                        break;
                    case 3:
                        if (logText[0] == "Right") logText.RemoveAt(0);
                        else { logText.Insert(0, "Left"); }
                        break;
                }
            }
            Debug.LogFormat("[Gnomish Puzzle #{0}] {1} lever pulled.", moduleId, num == 1 ? "Left" : num == 2 ? "Middle" : "Right");
            //GetComponent<KMAudio>().PlaySoundAtTransform("LeverPull", Lever.transform);
            for (int i = 0; i < 36; i++)
            {
                Lever.transform.Rotate(-2.5f, 0, 0);
                yield return new WaitForSeconds(0.0005f);
            }
            if (num != 2)
            {
                for (int i = 0; i < objectsToInteract.Count; i++)
                {
                    StartCoroutine(Flip(objectsToInteract[i], i, num));
                }
            }
            else
            {
                StartCoroutine(Flip(objectsToInteract[0], 2, num));
                StartCoroutine(Flip(objectsToInteract[1], 4, num));
            }
            for (int i = 0; i < 36; i++)
            {
                Lever.transform.Rotate(2.5f, 0, 0);
                yield return new WaitForSeconds(0.0005f);
            }
            yield return new WaitUntil(() => !objectDict[SymbolOBJs[0]] && !objectDict[SymbolOBJs[1]] && !objectDict[SymbolOBJs[2]] && !objectDict[SymbolOBJs[3]] && !objectDict[SymbolOBJs[4]] && !objectDict[SymbolOBJs[5]] && !objectDict[SymbolOBJs[6]]);
            ableToInteract = true;
            if (getSolve()) { Debug.LogFormat("[Gnomish Puzzle #{0}] All symbols are on the correct positions! Module solved!", moduleId); solved = true; GetComponent<KMBombModule>().HandlePass(); }
            if (first && judge)
            {
                first = false;
                bool _start = false;
                StartCoroutine(TextFader(() => _start));
                StartCoroutine(TextMover(() => _start));
                _start = true;
            }
        }
        yield break;
    }
    IEnumerator TextFader(Func<bool> pred)
    {
        yield return new WaitUntil(pred);
        for (int i = 0; i < 64; i++)
        {
            var comp = judgeText.GetComponent<TextMesh>();
            var c = new Color(comp.color.r, comp.color.g, comp.color.b, comp.color.a);
            c.a += 3.984375f / 255f;
            judgeText.GetComponent<TextMesh>().color = c;
            yield return new WaitForSeconds(0.0001f);
        }
        for (int i = 0; i < 64; i++)
        {
            var comp = judgeText.GetComponent<TextMesh>();
            var c = new Color(comp.color.r, comp.color.g, comp.color.b, comp.color.a);
            c.a -= 3.984375f / 255f;
            judgeText.GetComponent<TextMesh>().color = c;
            yield return new WaitForSeconds(0.0001f);
        }
    }
    IEnumerator TextMover(Func<bool> pred)
    {
        yield return new WaitUntil(pred);
        for (int i = 0; i < 128; i++)
        {
            judgeText.transform.localPosition = new Vector3(judgeText.transform.localPosition.x + 0.0002f, judgeText.transform.localPosition.y, judgeText.transform.localPosition.z);
            yield return new WaitForSeconds(0.0001f);
        }
    }
    IEnumerator Flip(GameObject Object, int id, int movenum)
    {
        yield return null;
        objectDict[Object] = true;
        if (movenum == 2) { yield return new WaitUntil(() => objectDict[SymbolOBJs[2]] && objectDict[SymbolOBJs[4]]); }
        else { yield return new WaitUntil(() => objectDict[SymbolOBJs[0]] && objectDict[SymbolOBJs[1]] && objectDict[SymbolOBJs[2]] && objectDict[SymbolOBJs[3]] && objectDict[SymbolOBJs[4]] && objectDict[SymbolOBJs[5]] && objectDict[SymbolOBJs[6]]); }
        for (int i = 0; i < 20; i++)
        {
            Object.transform.Rotate(Vector3.right, 9);
            yield return new WaitForSeconds(.015f);
        }
        Object.GetComponent<Renderer>().material = materialDict[SymbolNames[id]];
        for (int i = 0; i < 20; i++)
        {
            Object.transform.Rotate(Vector3.right, 9);
            yield return new WaitForSeconds(.015f);
        }
        if (SymbolNames[id] == finalOrder[id]) { LEDOBJs[id].GetComponent<Renderer>().material = Lit; }
        else { LEDOBJs[id].GetComponent<Renderer>().material = unLit; }
        objectDict[Object] = false;
        yield break;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        FinalizeLog();
        _forcesolve = true;
        yield return new WaitUntil(() => ableToInteract);
        List<IEnumerator> coros = new List<IEnumerator>();
        foreach (string lever in logText)
        {
            switch (lever)
            {
                case "Left":
                    yield return Interact(1, Levers[0], true);
                    break;
                case "Middle":
                    yield return Interact(2, Levers[1], true);
                    break;
                case "Right":
                    yield return Interact(3, Levers[2], true);
                    break;
            }
            yield return true;
        }
    }

#pragma warning disable 414
    [HideInInspector]
    public string TwitchHelpMessage = "Use '!{0} flip <name>,<name2>' Names can be: Left, Middle, Right, L, M, R, 1, 2 ,3 (Levers are numbered in reading order)";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if(_forcesolve)
        {
            yield return "sendtochaterror A force-solving is already in progress.";
            yield break;
        }
        string[] splitted = command.ToLowerInvariant().Replace("flip ", "").Split(',');
        List<KMSelectable> objects = new List<KMSelectable>();
        foreach (string item in splitted)
        {
            switch (item)
            {
                case "left":
                case "l":
                case "1":
                    objects.Add(Levers[0].GetComponent<KMSelectable>());
                    break;
                case "middle":
                case "m":
                case "2":
                    objects.Add(Levers[1].GetComponent<KMSelectable>());
                    break;
                case "right":
                case "r":
                case "3":
                    objects.Add(Levers[2].GetComponent<KMSelectable>());
                    break;
                default:
                    yield return null;
                    yield return "sendtochaterror Lever not valid!";
                    yield break;
                    break;
            }
        }
        foreach(var obj in objects)
        {
            yield return null;
            yield return new WaitUntil(() => ableToInteract);
            obj.OnInteract();
            yield return new WaitUntil(() => ableToInteract);
        }
    }
}
