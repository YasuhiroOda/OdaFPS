using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;

    public float mouseSensitivity = 2f;

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

    public float walkSpeed=4f;

    public float runSpeed = 8f;

    private float activeMovespeed=4f;

    //y軸の回転格納
    private float verticalMouseInput;

    private Camera cam;

    private bool cursorLock=true;

    //武器の格納リスト
    public List<Gun> guns = new List<Gun>();

    //洗濯中の武器管理用数値
    private int selectedGun = 0;

    //射撃間隔
    private float shotTimer;
    [Tooltip("所持弾薬")]
    public int[] ammunition;
    [Tooltip("最高所持弾薬数")]
    public int[] maxAmmunition;
    [Tooltip("マガジン内の弾数")]
    public int[] ammoClip;
    [Tooltip("マガジンに入る最大の数")]
    public int[] maxAmmoClip;

    private UIManager uIManager;//UI管理

    private void Awake()
    {
        uIManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
    }

    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        UpdateCursorLock();
    }

    private void Update()
    {
        //視点移動関数の呼び出し
        PlayerRotate();
        //移動関数を呼ぶ
        PlayerMove();
        if(IsGround())
        {
            //走る関数
            Run();
            //ジャンプ関数を呼ぶ
            Jump();
        }
        Aim();
        Fire();
        Reload();
        //武器の変更キー検知関数
        SwitchingGuns();
        //カーソルの表示非表示
        UpdateCursorLock();
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

    //初期設定では0.02秒ごとに呼ばれる
    private void FixedUpdate()
    {
        //弾薬テキスト更新
        uIManager.SettingBulletsText(ammoClip[selectedGun], ammunition[selectedGun]);
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
        //
    }

    public void Run()
    {
        //シフト押されている時にスピード切り替える
        if(Input.GetKey(KeyCode.LeftShift))
        {
            activeMovespeed = runSpeed;
        }
        else
        {
            activeMovespeed = walkSpeed;
        }
    }

    public void UpdateCursorLock()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLock = false;//表示
        }
        else if(Input.GetMouseButton(0))
        {
            cursorLock = true;//非表示
        }

        if(cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;

        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }


    }
    public void SwitchingGuns()
    {
        //ホイールクルクルで銃の切り替え
        if(Input.GetAxisRaw("Mouse ScrollWheel")>0f)
        {
            selectedGun++;
            if(selectedGun >= guns.Count)
            {
                selectedGun = 0;
            }
            //銃を切り替える関数
            SwitchGuns();
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel")<0f)
        {
            selectedGun--;
            if(selectedGun<0)
            {
                selectedGun = guns.Count - 1;
            }
            //銃を切り替える関数
            SwitchGuns();

        }
        
        //数値キー入力で銃の切り替え
        for (int i = 0; i < guns.Count; i++)
        {
            //数値キーを押したのか判定
            if(Input.GetKeyDown((i+1).ToString()))
            {
                //銃を切り替える
                selectedGun = i;
                SwitchGuns();
            }
        }
        
        /*
        if(Input.GetKeyDown("1"))
        {
            selectedGun = 0;
            SwitchGuns();
        }
        if (Input.GetKeyDown("2"))
        {
            selectedGun = 1;
            SwitchGuns();
        }
        if (Input.GetKeyDown("3"))
        {
            selectedGun = 2;
            SwitchGuns();
        }
        */
    }

    public void SwitchGuns()
    {
        foreach(Gun gun in guns)
        {
            gun.gameObject.SetActive(false);
        }
        guns[selectedGun].gameObject.SetActive(true);
    }

    public void Aim()
    {
        //右クリックの検知
        if (Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,
                guns[selectedGun].adsZoom,
                guns[selectedGun].adsSpeed*Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,
                60f,
                guns[selectedGun].adsSpeed * Time.deltaTime);
        }
    }
    public void Fire()
    {
        //撃ち出せるのか
        if(Input.GetMouseButton(0) && ammoClip[selectedGun]>0 && Time.time>shotTimer)
        {
            //弾を打ち出す関数呼び出し
            FiringBullet();
        }
    }
    public void FiringBullet()
    {
        //弾を減らす
        ammoClip[selectedGun]--;

        //光線を作る
        Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));

        if(Physics.Raycast(ray,out RaycastHit hit))
        {
            //Debug.Log("当たったオブジェクトは" + hit.collider.gameObject.name);
            //弾痕を当たった場所に生成
            GameObject bulletImpactObject = Instantiate(guns[selectedGun].bulletImpact,
                hit.point+(hit.normal*0.02f),
                Quaternion.LookRotation(hit.normal, Vector3.up));

            Destroy(bulletImpactObject, 10f);
        }
        //射撃間隔設定
        shotTimer = Time.time + guns[selectedGun].shootInterval;
    }
    private void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //リロードで補充する弾数を取得する
            int amountNeed = maxAmmoClip[selectedGun] - ammoClip[selectedGun];

            //必要な弾薬量と所持弾薬量を比較
            int ammoAvailable = amountNeed < ammunition[selectedGun] ? amountNeed : ammunition[selectedGun];

            //弾薬が満タンの時はリロードできない&弾薬を所持しているとき
            if (amountNeed != 0 && ammunition[selectedGun] != 0)
            {
                //所持弾薬からリロードする弾薬分を引く
                ammunition[selectedGun] -= ammoAvailable;
                //銃に装填する
                ammoClip[selectedGun] += ammoAvailable;
            }
        }
    }

}
