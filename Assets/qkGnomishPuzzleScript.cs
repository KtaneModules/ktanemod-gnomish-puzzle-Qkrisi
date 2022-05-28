using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Debug.LogFormat("[Gnomish Puzzle #{0}] Starting order: {1}", moduleId, SymbolNames.Join());
        Debug.LogFormat("[Gnomish Puzzle #{0}] The gnomes deem you {1}worthy.", moduleId, judge ? "" : "not ");
        Debug.LogFormat("[Gnomish Puzzle #{0}] Final order: {1}", moduleId, finalOrder.Join());
        FinalizeLog();
        logText.Reverse();
        Debug.LogFormat("[Gnomish Puzzle #{0}] One possible sequence to solve: {1}", moduleId, string.Join(", ", logText.ToArray()));
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
    private bool getSolve() { return SymbolNames.SequenceEqual(finalOrder); }

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
            if (!forceSolve)
            {
                if (logText.Any())
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
                Debug.LogFormat("[Gnomish Puzzle #{0}] Resulting in the current formation: {1}", moduleId, SymbolNames.Join());
            }
            //GetComponent<KMAudio>().PlaySoundAtTransform("LeverPull", Lever.transform);
            var amountToModify = Quaternion.Euler(-90, 0, 0);
            var startRotation = Lever.transform.localRotation;
            var resultingRotation = startRotation * amountToModify;
            for (float i = 0; i < 1; i += Time.deltaTime * 5)
            {
                Lever.transform.localRotation = Quaternion.Lerp(startRotation, resultingRotation, i);
                yield return null;
            }
            Lever.transform.localRotation = resultingRotation;
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
            for (float i = 0; i < 1; i += Time.deltaTime * 5)
            {
                Lever.transform.localRotation = Quaternion.Lerp(startRotation, resultingRotation, 1f - i);
                yield return null;
            }
            Lever.transform.localRotation = startRotation;
            yield return new WaitUntil(() => !objectDict[SymbolOBJs[0]] && !objectDict[SymbolOBJs[1]] && !objectDict[SymbolOBJs[2]] && !objectDict[SymbolOBJs[3]] && !objectDict[SymbolOBJs[4]] && !objectDict[SymbolOBJs[5]] && !objectDict[SymbolOBJs[6]]);
            ableToInteract = true;
            if (getSolve()) { if(!forceSolve) Debug.LogFormat("[Gnomish Puzzle #{0}] All symbols are on the correct positions! Module solved!", moduleId); solved = true; GetComponent<KMBombModule>().HandlePass(); }
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
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            var comp = judgeText.GetComponent<TextMesh>();
            comp.color = new Color(i, i, i, i);
            yield return null;
        }
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            var comp = judgeText.GetComponent<TextMesh>();
            comp.color = new Color(1f - i, 1f - i, 1f - i, 1f - i);
            yield return null;
        }
        judgeText.GetComponent<TextMesh>().color = Color.clear;
    }
    IEnumerator TextMover(Func<bool> pred)
    {
        yield return new WaitUntil(pred);
        var startTransform = judgeText.transform.localPosition;
        var resultingTransformLocal = startTransform + (Vector3.right * 0.0256f);
        for (float t = 0; t < 1f; t += Time.deltaTime / 2)
        {
            judgeText.transform.localPosition = Vector3.Lerp(startTransform, resultingTransformLocal, t);
            yield return null;
        }
        judgeText.transform.localPosition = resultingTransformLocal;
    }
    IEnumerator Flip(GameObject Object, int id, int movenum)
    {
        yield return null;
        objectDict[Object] = true;
        if (movenum == 2) { yield return new WaitUntil(() => objectDict[SymbolOBJs[2]] && objectDict[SymbolOBJs[4]]); }
        else { yield return new WaitUntil(() => objectDict[SymbolOBJs[0]] && objectDict[SymbolOBJs[1]] && objectDict[SymbolOBJs[2]] && objectDict[SymbolOBJs[3]] && objectDict[SymbolOBJs[4]] && objectDict[SymbolOBJs[5]] && objectDict[SymbolOBJs[6]]); }
        var rotationFlipAmt = Quaternion.Euler(Vector3.right * 180);
        var resultingRotation = Object.transform.localRotation * rotationFlipAmt;
        var startingRotation = Object.transform.localRotation;
        var startingRotationOrgin = Object.transform.localRotation;
        for (float t = 0; t < 1f; t += Time.deltaTime * 4)
        {
            Object.transform.localRotation = Quaternion.Lerp(startingRotation, resultingRotation, t);
            yield return null;
        }
        Object.transform.localRotation = resultingRotation;
        startingRotation = resultingRotation;
        resultingRotation = startingRotation * rotationFlipAmt;
        Object.GetComponent<Renderer>().material = materialDict[SymbolNames[id]];
        for (float t = 0; t < 1f; t += Time.deltaTime * 4)
        {
            Object.transform.localRotation = Quaternion.Lerp(startingRotation, resultingRotation, t);
            yield return null;
        }
        Object.transform.localRotation = startingRotationOrgin;
        if (SymbolNames[id] == finalOrder[id]) { LEDOBJs[id].GetComponent<Renderer>().material = Lit; }
        else { LEDOBJs[id].GetComponent<Renderer>().material = unLit; }
        objectDict[Object] = false;
        yield break;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        _forcesolve = true;
        yield return new WaitUntil(() => ableToInteract);
        FinalizeLog();
        Debug.LogFormat("[Gnomish Puzzle #{0}] Force-solving module...", moduleId);
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
    public string TwitchHelpMessage = "Use '!{0} flip <name>,<name2>' Names can be: Left, Middle, Right, L, M, R, 1, 2 ,3 (Levers are numbered 1-3, from left to right)";
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
            switch (item.Trim())
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
                    yield return "sendtochaterror You are attempting to pull a lever that doesn't exist! Command ignored.";
                    yield break;
            }
        }
        for (int a = 0; a < objects.Count; a++)
        {
            KMSelectable obj = objects[a];
            yield return null;
            do
                yield return string.Format("trycancel Your command has been canceled after {0} pull{1}.", a, a == 1 ? "" : "s");
            while (!ableToInteract);
            obj.OnInteract();
            do
                yield return string.Format("trycancel Your command has been canceled after {0} pull{1}.", a + 1, a == 0 ? "" : "s");
            while (!ableToInteract);
            yield return "solve";
        }
    }
}
