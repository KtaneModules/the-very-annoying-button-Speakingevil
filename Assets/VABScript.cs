using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class VABScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombInfo bomb;
    public KMSelectable button;
    public Renderer[] arrows;
    public Renderer screen;
    public TextMesh disp;
    public Material[] arrowcol;
    public Material[] buttoncol;
    public GameObject[] hatch;

    private static string[] exempt = null;
    private static string[] collog = new string[4] { "Red", "Yellow", "Green", "Blue" };
    private int submission;
    private int buttoncolour;
    private int remaining = -1;
    private int counter = 1;
    private int[] record = new int[4];
    private int[][] valid = new int[4][] { new int[5] { 5, 4, 3, 2, 1 }, new int[5] { 9, 7, 5, 3, 1 }, new int[5] { 8, 7, 4, 2, 1 }, new int[5] { 9, 8, 6, 4, 1 } };
    private bool solvable;
    private bool open;
    private bool pressable;
    private bool movehatch;
    private bool submitted;
    private bool twitchActive;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        button.OnInteract += delegate () { ButtonPress(button); return false; };
        exempt = GetComponent<KMBossModule>().GetIgnoredModules("The Very Annoying Button", new string[]
        {
            "Forget Me Not",
            "Forget Everything",
            "Forget This",
            "Forget Infinity",
            "Forget Them All",
            "Simon's Stages",
            "Turn The Key",
            "The Time Keeper",
            "Timing is Everything",
            "Alchemy",
            "Cookie Jars",
            "Purgatory",
            "Hogwarts",
            "Souvenir",
            "The Swan",
            "Divided Squares",
            "The Troll",
            "Tallordered Keys",
            "Forget Enigma",
            "Forget Us Not",
            "Organization",
            "Forget Perspective",
            "The Very Annoying Button",
            "Forget Me Later",
            "The Mildly Annoying Button",
            "The Task Master",
            "Simon Supervises",
            "Bad Mouth",
            "Bad TV",
            "Simon Superintends"
        });
    }

    private void Start()
    {
        hatch[0].transform.localEulerAngles = new Vector3(0, 0, 0);
        hatch[1].transform.localEulerAngles = new Vector3(0, 0, 180);
        foreach (Renderer r in arrows)
        {
            r.material = arrowcol[1];
        }
        GetComponent<KMBombModule>().OnActivate = OnActivate;
    }

    private void OnActivate()
    {
        if (TwitchPlaysActive)
        {
            twitchActive = true;
        }
        else
        {
            twitchActive = false;
        }
        StartCoroutine(Reset());
        Debug.LogFormat("[The Very Annoying Button #{0}] Twitch Plays mode: {1}", moduleId, TwitchPlaysActive);
    }

    private IEnumerator Reset()
    {
        for (int i = 0; i < 11; i++)
        {
            if (i > 0)
            {
                remaining = 10 - i;
                if (open == true && pressable == false)
                {
                    pressable = true;
                }
                if (i > 1)
                {
                    arrows[i - 2].material = arrowcol[1];
                    arrows[i + 7].material = arrowcol[1];
                }
                if (i > 9)
                {
                    i = -1;
                }
                if (i != -1)
                {
                    if (twitchActive && open && i == 1 && !solvable)
                    {
                        yield return new WaitForSeconds(9);
                    }
                    yield return new WaitForSeconds(1);
                }
            }
            else
            {
                if (i == 0)
                {
                    if (open == true)
                    {
                        if (solvable == false)
                        {
                            open = false;
                            movehatch = true;
                            disp.text = string.Empty;
                            button.GetComponent<Renderer>().material = arrowcol[1];
                        }
                        if (submitted == false && solvable == false)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            screen.material = arrowcol[0];
                            Debug.LogFormat("[The Very Annoying Button #{0}] Error: Button not pressed", moduleId);
                        }
                        else if (submitted == true && !valid[buttoncolour].Where(x => !record.Contains(x)).Contains(submission))
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            screen.material = arrowcol[0];
                            Debug.LogFormat("[The Very Annoying Button #{0}] Error: Invalid submission", moduleId);
                        }
                        else if (submitted == true)
                        {
                            if (solvable == true)
                            {
                                GetComponent<KMBombModule>().HandlePass();
                                screen.material = arrowcol[3];
                                disp.text = string.Empty;
                                button.GetComponent<Renderer>().material = arrowcol[1];
                                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                                Debug.Log("OK");
                                moduleSolved = true;
                            }
                            else
                            {
                                record[buttoncolour] = submission;
                                Debug.LogFormat("[The Very Annoying Button #{0}] OK: The {2} button has recorded the value {1}", moduleId, submission, collog[buttoncolour]);
                                Audio.PlaySoundAtTransform("InputCorrect", transform);
                            }
                        }
                        submitted = false;
                        pressable = false;
                        submission = -1;
                    }
                    else
                    {
                        int r = Random.Range(0, 10);
                        if (solvable == true || r < counter)
                        {
                            counter = Mathf.FloorToInt(4 * bomb.GetSolvedModuleNames().Where(x => !exempt.Contains(x)).Count() / (bomb.GetSolvableModuleNames().Where(x => !exempt.Contains(x)).Count() + 1)) - 1;
                            if (open == false)
                            {
                                open = true;
                                movehatch = true;
                            }
                            if (movehatch == false)
                            {
                                movehatch = true;
                            }
                            buttoncolour = Random.Range(0, 4);
                            disp.text = "RYGB"[buttoncolour].ToString();
                            button.GetComponent<Renderer>().material = buttoncol[buttoncolour];
                            switch (buttoncolour)
                            {
                                case 0:
                                    disp.color = new Color32(255, 0, 0, 255);
                                    break;
                                case 1:
                                    disp.color = new Color32(255, 255, 0, 255);
                                    break;
                                case 2:
                                    disp.color = new Color32(0, 255, 0, 255);
                                    break;
                                case 3:
                                    disp.color = new Color32(0, 0, 255, 255);
                                    break;
                            }
                            Debug.LogFormat("[The Very Annoying Button #{0}] The button had turned {1} at {2}", moduleId, collog[buttoncolour], bomb.GetFormattedTime());
                        }
                        else
                        {
                            counter++;
                        }
                    }
                    if (movehatch == true)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (open == true)
                            {
                                hatch[0].transform.localEulerAngles += new Vector3(0, 0, 4);
                                hatch[1].transform.localEulerAngles -= new Vector3(0, 0, 4);
                            }
                            else
                            {
                                hatch[0].transform.localEulerAngles -= new Vector3(0, 0, 4);
                                hatch[1].transform.localEulerAngles += new Vector3(0, 0, 4);
                            }
                            yield return new WaitForSeconds(0.1f);
                        }
                        movehatch = false;
                    }
                    if (bomb.GetSolvableModuleNames().Where(x => !exempt.Contains(x)).Count() == bomb.GetSolvedModuleNames().Count() || bomb.GetTime() < 60)
                    {
                        solvable = true;
                    }
                    if (moduleSolved == true)
                    {
                        break;
                    }
                }
                for (int j = 0; j < 9; j++)
                {
                    arrows[j].material = arrowcol[0];
                    arrows[j + 9].material = arrowcol[0];
                    yield return new WaitForSeconds(0.1f);
                }
                screen.material = arrowcol[1];
            }
        }
        for (int j = 0; j < 18; j++)
        {
            if (j < 9)
            {
                arrows[j].material = arrowcol[3];
                arrows[j + 9].material = arrowcol[3];
            }
            else
            {
                arrows[j - 9].material = arrowcol[1];
                arrows[j].material = arrowcol[1];
            }
            if (j == 17)
            {
                j = -1;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ButtonPress(KMSelectable b)
    {
        if (!submitted && remaining > 0 && pressable && open)
        {
            submitted = true;
            submission = remaining;
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, b.transform);
            Debug.LogFormat("[The Very Annoying Button #{0}] Submitted value = {1}", moduleId, submission);
            if (solvable == false)
            {
                b.GetComponent<Renderer>().material = arrowcol[1];
            }
            arrows[9 - remaining].material = arrowcol[2];
            arrows[18 - remaining].material = arrowcol[2];
        }
    }

    //twitch plays
    bool TwitchPlaysActive;

    private bool validCmd(string[] cmds)
    {
        string[] valids = { "1","2","3","4","5","6","7","8","9" };
        for(int i = 1; i < cmds.Length; i++)
        {
            if (!valids.Contains(cmds[i]))
            {
                return false;
            }
        }
        return true;
    }

    private List<int> parseVals(string[] cmds)
    {
        List<int> ints = new List<int>();
        for (int i = 1; i < cmds.Length; i++)
        {
            int temp = 0;
            int.TryParse(cmds[i], out temp);
            ints.Add(temp);
        }
        ints.Sort();
        return ints;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <#> (#)... [Press the button when the number of seconds remaining on the counter is <#> (or (#) and so on)] | Reference currently lit arrows for seconds remaining (1-9), actual seconds remaining is slightly extended in TP";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (!pressable)
        {
            yield return "sendtochaterror TVAB is not active";
            yield break;
        }
        if (submitted)
        {
            yield return "sendtochaterror Entry has already been submitted";
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length >= 2)
            {
                if (validCmd(parameters))
                {
                    yield return null;
                    List<int> parsedvals = parseVals(parameters);
                    int waittime = 0;
                    parsedvals.Sort();
                    if (!solvable)
                        for (int i = parsedvals.Count - 1; i >= 0; i--)
                        {
                            if (remaining > parsedvals[i] || (remaining == 9 && parsedvals[i] == 9))
                            {
                                waittime = parsedvals[i];
                                break;
                            }
                        }
                    else
                        waittime = parsedvals.Last();
                    while (solvable && !submitted)
                    {
                        while (remaining != waittime)
                        {
                            yield return "trycancel Button submission cancelled due to a cancel request.";
                            yield return new WaitForSeconds(0.1f);
                        }
                        if (waittime != 0)
                        {
                            button.OnInteract();
                        }
                        else if(!solvable)
                        {
                            yield return "sendtochaterror Button times missed! Press command cancelled.";
                        }
                    }
                }
            }
            yield break;
        }
    }

    void TwitchHandleForcedSolve()
    {
        StartCoroutine(HandleSolve());
    }

    IEnumerator HandleSolve()
    {
        while (!moduleSolved)
        {
            while (!open || !pressable) { yield return new WaitForSeconds(0.1f); }
            List<int> valids = new List<int>();
            for (int i = 9; i > 0; i--)
            {
                if (valid[buttoncolour].Where(x => !record.Contains(x)).Contains(i))
                {
                    valids.Add(i);
                }
            }
            valids.Sort();
            if (remaining < valids[0] && !solvable)
            {
                submitted = true;
                pressable = false;
                submission = valids[Random.Range(0, valids.Count)];
            }
            else
            {
                while (!valids.Contains(remaining)) { yield return new WaitForSeconds(0.1f); }
                button.OnInteract();
            }
            yield return new WaitForSeconds(0.001f);
        }
    }
}
