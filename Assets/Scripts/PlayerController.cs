using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;

    public float mouseSensitivity = 1f;

    //ユーザーのマウス入力を格納
    private Vector2 mouseInpt;

    private Vector3 moveDir;

    private Vector3 movement;

    public Vector3 jumpForce = new Vector3(0, 6, 0);
    //レイを飛ばすオブジェクトの位置
    public Transform groundCheckPoint;
    //地面レイヤー
    public LayerMask groundLayers;

    private Rigidbody rb;

    private float activeMovespeed=4f;

    //y軸の回転格納
    private float verticalMouseInput;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //視点移動関数の呼び出し
        PlayerRotate();
        //移動関数を呼ぶ
        PlayerMove();
        //ジャンプ関数を呼ぶ
        Jump();
    }

    public void PlayerRotate()
    {
        //変数にユーザーのマウスの動きを格納
        mouseInpt = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity, Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        //マウスのx軸の動きを反映
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
            transform.eulerAngles.y + mouseInpt.x,
            transform.eulerAngles.z);
        
        //y軸の値に現在の値を足す
        verticalMouseInput += mouseInpt.y;
        //数値を丸める
        verticalMouseInput = Mathf.Clamp(verticalMouseInput, -60f, 60f);

        viewPoint.rotation = Quaternion.Euler(-verticalMouseInput,
            viewPoint.transform.rotation.eulerAngles.y,
            viewPoint.transform.rotation.eulerAngles.z);
        
    }

    private void LateUpdate()
    {
        //カメラの位置調整
        cam.transform.position = viewPoint.position;
        //回転
        cam.transform.rotation = viewPoint.rotation;
        
    }

    public void PlayerMove()
    {
        //移動用キーの入力を検知して値を格納する
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //進む方向を出して変数に格納
        movement = (transform.forward * moveDir.z) + (transform.right * moveDir.x).normalized;
        //現在位置に反映していく
        transform.position += movement * activeMovespeed * Time.deltaTime;
    }

    public void Jump()
    {
        //地面についていて、スペースキーが押された時
        if (IsGround() && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(jumpForce, ForceMode.Impulse);
        }
    }

    //地面についていればtrue
    public bool IsGround()
    {
        //判定してbool値を返す(レーザー飛ばすポジション、方向、距離、地面判定するレイヤー)
        return Physics.Raycast(groundCheckPoint.position,Vector3.down,0.25f,groundLayers);
    }
}
