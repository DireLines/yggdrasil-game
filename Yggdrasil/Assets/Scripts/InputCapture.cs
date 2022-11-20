using System.IO;	// for stream
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;	// for List compare
using UnityEngine.Events;

// public types for delegates
public delegate bool AnyKeyType();
public delegate bool GetKeyType(KeyCode code);
public delegate bool GetMouseButtonType(int button);
public delegate bool GetButtonType(string name);
public delegate float GetAxisType(string name);
public delegate Vector3 Vector3Type();
public delegate Vector2 Vector2Type();

// element of input sequence
public struct InputFrame {      // var names are reduced for smaller json
    public float t;         // time
    public List<KeyCode> gK;    // getKey
    public List<KeyCode> gKD;   // getKeyDown
    public List<KeyCode> gKU;   // getKeyUp
    public Vector3 mP;      // mousePosition
    public Vector3 mWP;     // mouseWorldPosition
    public Vector2 mSD;     // mouseScrollDelta
    public List<string> vB;     // virtual Button
    public List<string> vBD;    // virtual Button Down
    public List<string> vBU;    // virtual Button Up
    public List<float> vA;      // virtual Axis

    public void init() {
        gK = new List<KeyCode>();
        gKD = new List<KeyCode>();
        gKU = new List<KeyCode>();
        mP = new Vector3();
        mWP = new Vector3();
        mSD = new Vector2();
        vB = new List<string>();
        vBD = new List<string>();
        vBU = new List<string>();
        vA = new List<float>();
    }
};

public abstract class InputStream : MonoBehaviour {
    // public methods and properties for input access
    public GetKeyType GetKey;
    public GetKeyType GetKeyDown;
    public GetKeyType GetKeyUp;
    public GetMouseButtonType GetMouseButton;
    public GetMouseButtonType GetMouseButtonDown;
    public GetMouseButtonType GetMouseButtonUp;
    public GetButtonType GetButton;
    public GetButtonType GetButtonDown;
    public GetButtonType GetButtonUp;
    public GetAxisType GetAxis;
    public GetAxisType GetAxisRaw;

    protected Vector3Type _mousePosition;
    protected Vector3Type _mouseWorldPosition;
    protected Vector2Type _mouseScrollDelta;
    public Vector3 mousePosition { get { return _mousePosition(); } }
    public Vector3 mouseWorldPosition { get { return _mouseWorldPosition(); } }
    public Vector2 mouseScrollDelta { get { return _mouseScrollDelta(); } }

    protected AnyKeyType _anyKey;
    protected AnyKeyType _anyKeyDown;
    public bool anyKey { get { return _anyKey(); } }
    public bool anyKeyDown { get { return _anyKeyDown(); } }
}


public class StdInputStream : InputStream {
    public StdInputStream() {
        GetKey = Input.GetKey;
        GetKeyDown = Input.GetKeyDown;
        GetKeyUp = Input.GetKeyUp;
        GetMouseButton = Input.GetMouseButton;
        GetMouseButtonDown = Input.GetMouseButtonDown;
        GetMouseButtonUp = Input.GetMouseButtonUp;

        GetButton = Input.GetButton;
        GetButtonDown = Input.GetButtonDown;
        GetButtonUp = Input.GetButtonUp;
        GetAxis = Input.GetAxisRaw;

        _mousePosition = () => Input.mousePosition;
        _mouseWorldPosition = () => Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mouseScrollDelta = () => Input.mouseScrollDelta;

        _anyKey = () => Input.anyKey;
        _anyKeyDown = () => Input.anyKeyDown;
    }
}
public class EmptyInputStream : InputStream {
    public EmptyInputStream() {
        GetKey = (keyCode) => false;
        GetKeyDown = (keyCode) => false;
        GetKeyUp = (keyCode) => false;
        GetMouseButton = (button) => false;
        GetMouseButtonDown = (button) => false;
        GetMouseButtonUp = (button) => false;

        GetButton = (name) => false;
        GetButtonDown = (name) => false;
        GetButtonUp = (name) => false;
        GetAxis = (name) => 0f;

        _mousePosition = () => Vector3.zero;
        _mouseWorldPosition = () => Camera.main.ScreenToWorldPoint(Vector3.zero);
        _mouseScrollDelta = () => Vector2.zero;

        _anyKey = () => false;
        _anyKeyDown = () => false;
    }
}
public class StreamReadingFromFrame : InputStream {
    public void AttachToInputCapture(InputCapture capture) {
        GetKey = (keyCode) => GetKeyCodeInList(keyCode, capture.GetCurrentFrame().gK);
        GetKeyDown = (keyCode) => GetKeyCodeInList(keyCode, capture.GetCurrentFrame().gKD);
        GetKeyUp = (keyCode) => GetKeyCodeInList(keyCode, capture.GetCurrentFrame().gKU);
        GetMouseButton = (button) => GetKeyCodeInList(KeyCode.Mouse0 + button, capture.GetCurrentFrame().gK);
        GetMouseButtonDown = (button) => GetKeyCodeInList(KeyCode.Mouse0 + button, capture.GetCurrentFrame().gKD);
        GetMouseButtonUp = (button) => GetKeyCodeInList(KeyCode.Mouse0 + button, capture.GetCurrentFrame().gKU);

        GetButton = (name) => capture.GetCurrentFrame().vB.Contains(name);
        GetButtonDown = (name) => capture.GetCurrentFrame().vBD.Contains(name);
        GetButtonUp = (name) => capture.GetCurrentFrame().vBU.Contains(name);
        GetAxis = (name) => capture.GetCurrentFrame().vA.ElementAt(capture.AxisList.FindIndex(str => str == name));

        _mousePosition = () => capture.GetCurrentFrame().mP;
        _mouseWorldPosition = () => capture.GetCurrentFrame().mWP;
        _mouseScrollDelta = () => capture.GetCurrentFrame().mSD;

        _anyKey = () => capture.GetCurrentFrame().gK.Any<KeyCode>();
        _anyKeyDown = () => capture.GetCurrentFrame().gKD.Any<KeyCode>();
    }
    private bool GetKeyCodeInList(KeyCode code, List<KeyCode> list) {
        foreach (KeyCode vkey in list) {
            if (vkey == code)
                return true;
        }
        return false;
    }
}

public class InputCapture : InputStream {
    public UnityEvent OnSequenceEnd;
    //public InputCapture(InputCapture other) {
    //    active = other.active;
    //    recording = other.recording;
    //    playing = other.playing;
    //    UpdateCycle = other.UpdateCycle;
    //    AxisList = other.AxisList;
    //    ButtonList = other.ButtonList;
    //    oldSequence = other.oldSequence;
    //    currentSequence = other.currentSequence;
    //    nextSequence = other.nextSequence;
    //    listeningStream = other.listeningStream;
    //    playback = other.playback;
    //    record = other.record;
    //TODO: complete copy constructor
    //}
    // public types
    public enum UpdateFunction { FixedUpdate, Update, Both };

    // config
    public bool active = true;
    public bool recording = false;
    public bool playing = false;
    public UpdateFunction UpdateCycle = UpdateFunction.Update;
    // virtual button and axis support, list the InputManager's inputs you want to track
    public List<string> AxisList = new List<string>();
    public List<string> ButtonList = new List<string>();

    // Input sequences
    private InputFrame oldFrame;
    private InputFrame currentFrame;
    private InputFrame nextFrame;

    private InputStream listeningStream;
    private StreamReadingFromFrame playbackStream;
    private InputStream emptyStream;

    // delegate to switch from record to play activity
    private delegate void Work(float time);
    private Work playback;
    private Work record;

    private float startTimeRecord = 0.0f;
    private float startTimePlayback = 0.0f;

    Dictionary<string, List<InputFrame>> sequences;
    string writeSequenceName;
    string readSequenceName;
    private void Awake() {
        sequences = new Dictionary<string, List<InputFrame>>();
        listeningStream = gameObject.AddComponent<StdInputStream>();
        emptyStream = gameObject.AddComponent<EmptyInputStream>();
        playbackStream = gameObject.AddComponent<StreamReadingFromFrame>();
        playbackStream.AttachToInputCapture(this);

        //TODO: read saved sequences from file
        playback = idle;
        record = idle;
        SetOutputPassthrough();//TODO: keep stack of sequences, place std stream onto beginning of stack
    }

    ~InputCapture() {
        StopRecord();
        StopPlayback();
    }

    // Update is called once per frame
    void Update() {
        if (UpdateCycle == UpdateFunction.Update || UpdateCycle == UpdateFunction.Both) {
            record(Time.time - startTimeRecord);
            playback(Time.time - startTimePlayback);
        }
    }

    // FixedUpdate is called every physics update
    void FixedUpdate() {
        if (UpdateCycle == UpdateFunction.FixedUpdate || UpdateCycle == UpdateFunction.Both) {
            record(Time.time - startTimeRecord);
            playback(Time.time - startTimePlayback);
        }
    }
    public void StartRecord(string sequenceName) {
        print("recording " + sequenceName);
        writeSequenceName = sequenceName;
        recording = true;
        record = Record(sequenceName);
        SetOutputPassthrough();
    }
    public string StopRecord() {
        if (!recording) {
            return "";
        }
        print("stop record");
        sequences[writeSequenceName].Add(currentFrame);//add ending frame
        SetOutputIgnored();
        recording = false;
        record = idle;
        return writeSequenceName;
    }
    public void StartPlayback(string sequenceName, float playbackSpeed = 1f) {
        if (!HasSequence(sequenceName)) {
            print(gameObject.name + " does not have " + sequenceName);
            return;
        }
        print("playing " + sequenceName);
        readSequenceName = sequenceName;
        playing = true;
        playback = Play(sequenceName, playbackSpeed);
        SetOutputFromSequence();//TODO: keep stack of sequences, place current sequence onto stack
    }
    public string StopPlayback() {
        if (!playing) {
            return "";
        }
        print("stopping " + readSequenceName);
        SetOutputPassthrough();//TODO: keep stack of sequences, pop from stack
        playing = false;
        playback = idle;
        return readSequenceName;
    }
    public InputStream GetListeningStream() {
        return listeningStream;
    }
    public InputFrame GetCurrentFrame() {
        return currentFrame;
    }
    public InputStream GetPlaybackStream() {
        return playing ? playbackStream : emptyStream;
    }
    public void setListeningStream(InputStream stream) {
        listeningStream = stream;
    }
    public Dictionary<string, List<InputFrame>> GetSequences() {
        return sequences;
    }
    public bool HasSequence(string sequenceName) {
        return sequences.Keys.Contains(sequenceName);
    }
    public void SetSequences(Dictionary<string, List<InputFrame>> s) {
        sequences = new Dictionary<string, List<InputFrame>>(s);
    }
    private void idle(float time) {
        // nothing to do
    }
    public void Save(string sequenceName) {
        //TODO: write sequences[sequenceName] to json
    }
    Work Play(string sequenceName, float playbackSpeed = 1f) {
        startTimePlayback = Time.time;
        int index = 0;
        currentFrame = sequences[sequenceName][0];
        nextFrame = sequences[sequenceName][0];
        return (float time) => {
            if (time * playbackSpeed >= nextFrame.t) {
                oldFrame = currentFrame;
                currentFrame = nextFrame;

                index++;
                if (index >= sequences[sequenceName].Count) {
                    StopPlayback();
                    OnSequenceEnd?.Invoke();
                    Debug.Log("End of Sequence");
                } else {
                    nextFrame = sequences[sequenceName][index];
                }
            }
        };
    }
    void FillInputFrame(ref InputFrame frame, float time) {
        frame.t = time;

        // store only true boolean
        foreach (KeyCode vkey in System.Enum.GetValues(typeof(KeyCode))) {
            if (listeningStream.GetKey(vkey))
                frame.gK.Add(vkey);
            if (listeningStream.GetKeyDown(vkey))
                frame.gKD.Add(vkey);
            if (listeningStream.GetKeyUp(vkey))
                frame.gKU.Add(vkey);
        }

        frame.mP = listeningStream.mousePosition;

        frame.mWP = Camera.main.ScreenToWorldPoint(frame.mP);

        frame.mSD = listeningStream.mouseScrollDelta;

        foreach (string virtualAxis in AxisList)
            frame.vA.Add(listeningStream.GetAxis(virtualAxis));

        foreach (string ButtonName in ButtonList) {
            if (listeningStream.GetButton(ButtonName))
                frame.vB.Add(ButtonName);
            if (listeningStream.GetButtonDown(ButtonName))
                frame.vBD.Add(ButtonName);
            if (listeningStream.GetButtonUp(ButtonName))
                frame.vBU.Add(ButtonName);
        }
    }
    Work Record(string sequenceName) {
        startTimeRecord = Time.time;
        sequences[sequenceName] = new List<InputFrame>();
        oldFrame.init();
        FillInputFrame(ref oldFrame, 0);//initial state
        sequences[sequenceName].Add(oldFrame);
        return (float time) => {
            currentFrame.init();
            FillInputFrame(ref currentFrame, time);
            // only write if something changed
            if (AnyChange(oldFrame, currentFrame)) {
                oldFrame = currentFrame;
                sequences[sequenceName].Add(currentFrame);
            }
        };
    }
    List<InputFrame> Load(string sequenceName) {
        return new List<InputFrame>();
    }
    // check if any control settings have changed
    private bool AnyChange(InputFrame a, InputFrame b) {
        if (!Enumerable.SequenceEqual(a.gK, b.gK)) return true;
        else if (!Enumerable.SequenceEqual(a.vB, b.vB)) return true;
        else if (!Enumerable.SequenceEqual(a.vA, b.vA)) return true;
        else if (a.mP != b.mP) return true;
        else if (a.mWP != b.mWP) return true;
        else if (a.mSD != b.mSD) return true;
        else return false;
    }

    void SetOutputToStream(InputStream stream) {
        GetKey = stream.GetKey;
        GetKeyDown = stream.GetKeyDown;
        GetKeyUp = stream.GetKeyUp;
        GetMouseButton = stream.GetMouseButton;
        GetMouseButtonDown = stream.GetMouseButtonDown;
        GetMouseButtonUp = stream.GetMouseButtonUp;

        GetButton = stream.GetButton;
        GetButtonDown = stream.GetButtonDown;
        GetButtonUp = stream.GetButtonUp;
        GetAxis = stream.GetAxis;

        _mousePosition = () => stream.mousePosition;
        _mouseWorldPosition = () => Camera.main.ScreenToWorldPoint(stream.mousePosition);
        _mouseScrollDelta = () => stream.mouseScrollDelta;

        _anyKey = () => stream.anyKey;
        _anyKeyDown = () => stream.anyKeyDown;
    }
    // redirect public methods and properties to the listening stream
    public void SetOutputPassthrough() {
        SetOutputToStream(listeningStream);
    }
    public void SetOutputIgnored() {
        SetOutputToStream(emptyStream);
    }
    // redirect public methods and properties to our replay system
    public void SetOutputFromSequence() {
        SetOutputToStream(playbackStream);
    }
    private bool GetKeyCodeInList(KeyCode code, List<KeyCode> list) {
        foreach (KeyCode vkey in list) {
            if (vkey == code)
                return true;
        }
        return false;
    }
}
