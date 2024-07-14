/*
 * Unity社が提供するGamekit3D内のSimpleTransformerの機能拡張スクリプトです。
 * Splinesによって作られた軌跡に沿って足場を移動するスクリプトになります。
 *
 * (C)2024 slip
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 * [Twitter]: https://twitter.com/kjmch2s/
 *
 * 利用規約：
 *  作者に無断で改変、再配布が可能で、利用形態（商用、18禁利用等）
 *  についても制限はありません。
 *  このスクリプトはもうあなたのものです。
 * 
 */

using UnityEngine;
using UnityEngine.Splines;
using Gamekit3D.GameCommands;
using System.Collections;
using System.Collections.Generic;

public class MoveAlongSpline:SimpleTransformer
{
    [SerializeField]
    private List<Rigidbody> rigidbodys = null;

    [SerializeField]
    private SplineContainer splineContainer;

    [SerializeField]
    private bool m_IsForwardDirection = true;

    [SerializeField]
    private bool m_IsRotatablePlatform = true;

    private float progress_rate = 0.0f;

    private float progress_length = 0.0f;

    private float pos_base = 0.0f;
    private float pre_pos = 0.0f;
    
    public bool IsForwardDirection{
        set{this.m_IsForwardDirection = value;}
        get{return this.m_IsForwardDirection;}
    }

    protected override void Awake()
    {
        base.Awake();
        this.pos_base = 0.0f;
        this.pre_pos = 0.0f;

        int iNum = 0;

        foreach(Rigidbody rb in rigidbodys){
            rb.gameObject.GetComponent<Platform>().Initialize();
            float pos_rb = 0.0f;
            pos_rb = this.pos_base + (float)iNum/(float)rigidbodys.Count;
            this.MoveSinglePlatform(pos_rb,rb);
            iNum = iNum + 1;
        }
    }

    public override void PerformTransform(float position)
    {
        int iNum = 0;
        float iDirection = 1.0f;

        if(this.m_IsForwardDirection){
            iDirection = 1.0f;
        }
        else{
            iDirection = -1.0f;
        }

        this.pos_base = this.pos_base + (position - this.pre_pos) * iDirection;
        this.pre_pos = position;

        if(this.pos_base >= 1.0f){
            this.pos_base = this.pos_base - 1.0f;
        }
        else if(this.pos_base <= 0.0f){
            this.pos_base =this.pos_base+ 1.0f;
        }

        foreach(Rigidbody rb in rigidbodys){
            
            float pos_rb = 0.0f;
            pos_rb = this.pos_base + (float)iNum/(float)rigidbodys.Count;

            float temp = pos_rb;

            if(pos_rb >= 1.0f){
                pos_rb = pos_rb - 1.0f;
            }
            else if(pos_rb <= 0.0f){
                pos_rb = pos_rb + 1.0f;
            } 

            this.MoveSinglePlatform(pos_rb,rb);

            iNum = iNum + 1;
        }
    }

    private void MoveSinglePlatform(float position,Rigidbody target){

        float position_accel = 0.0f;

        if(accelCurve != null){
            position_accel = accelCurve.Evaluate(position);
        }

        splineContainer.Evaluate(position_accel
        ,out var curvePosition
        ,out var tangent
        ,out var upVector
        );
        //Debug.Log(target.gameObject.name + ":"+ tangent);
        //tangent = tangent * new Vector3(1,0,1);
        
        var pos = (Vector3)curvePosition;
        var rot = Quaternion.LookRotation(tangent,upVector);
        
        //Debug.Log(target.gameObject.name + ":" + (rot.eulerAngles.y - target.rotation.eulerAngles.y));

        Vector3 deltaPosition = pos - target.position;
        Quaternion deltaRotation = Quaternion.Inverse(target.rotation) * rot;
        //Quaternion deltaRotation = Quaternion.Inverse(target_QuaRot_xz) * rot_y;
        
        //エディタ用
        if (Application.isEditor && !Application.isPlaying){
            target.transform.position = pos;

            if(this.m_IsRotatablePlatform){
                target.transform.rotation = rot;
            }
        }
        
        target.MovePosition(pos);

        if(this.m_IsRotatablePlatform){
            target.MoveRotation(rot);
        }

        if (target.gameObject != null && this.m_IsRotatablePlatform){
            target.gameObject.GetComponent<Platform>().RotateCharacterController(deltaRotation);
        }
    }

}
