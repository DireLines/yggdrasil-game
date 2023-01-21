using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatedObject : MonoBehaviour {
    protected bool activated = false;
    protected bool visible = false;
    public virtual void SetVisible(bool visible) {
        this.visible = visible;
    }
    public virtual void SetActivated(bool activated) {
        this.activated = activated;
        if (activated) {
            SetVisible(true);
        }
    }
}
