/*
 * 该类用于实现场景中的关键帧动画效果
 * 通过委托函数添加动画
 * 内部采用Update逐帧播放动画
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorObject : MonoBehaviour {

    public delegate void PlayingDelegate();

    public PlayingDelegate playing;

	// Use this for initialization
	void Start () 
    {
        playing = new PlayingDelegate(Playing);
	}
	
	// Update is called once per frame
	void Update () 
    {
        //playing();
	}

    void FixedUpdate()
    {
        playing();
    }

    void Playing()
    {
        //do nothing
    }
}
