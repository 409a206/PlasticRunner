using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    public static float NARAK_HEIGHT = -5.0f;
    public static float ACCELERATION = 10.0f;               //가속도
    public static float SPEED_MIN = 4.0f;                   //속도의 최솟값
    public static float SPEED_MAX = 8.0f;                   //속도의 최댓값
    public static float JUMP_HEIGHT_MAX = 3.0f;             //점프 높이
    public static float JUMP_KEY_RELEASE_REDUCE = 0.5f;     //점프 후의 감속도

    public enum STEP {                                      //Player의 각종 상태를 나타내는 자료형
        NONE = -1,                                          //상태정보 없음
        RUN = 0,                                            //달린다
        JUMP,        //1                                    //점프
        MISS,        //2                                    //실패
        NUM,         //3 자동할당                           //상태가 몇 종류 있는지 보여준다 (=3)
    };

    public STEP step = STEP.NONE;
    public STEP next_step = STEP.NONE;

    public float step_timer = 0.0f;
    private bool is_landed = false;
    private bool is_collided = false;
    private bool is_key_released = false;

    public float current_speed = 0.0f;                      //현재 속도
    public LevelControl level_control = null;               //LevelControl이 저장됨

    private float click_timer = -1.0f;                      //버튼이 눌린 후의 시간
    private float CLICK_GRACE_TIME = 0.5f;                  //점프하고 싶은 의사를 받아들일 시간

    // Start is called before the first frame update
    void Start()
    {
        this.next_step = STEP.RUN;
       
    }

    // Update is called once per frame
    void Update()
    {   
        //this.transform.Translate(new Vector3(0.0f, 0.0f, 3.0f * Time.deltaTime));   
        Vector3 velocity = this.GetComponent<Rigidbody>().velocity;                                        //속도를 설정
        this.current_speed = this.level_control.getPlayerSpeed();
        this.check_landed();                                                             //착지 상태인지 체크

        switch(this.step) {  //달리는중 혹은 점프중일때
            case STEP.RUN : 
            case STEP.JUMP : 
                //현재 위치가 한계치 보다 아래면
                if(transform.position.y < NARAK_HEIGHT) {
                    this.next_step = STEP.MISS;      //실패 상태로 한다
                }
                break;
        }



        this.step_timer += Time.deltaTime;                                               //경과 시간을 진행한다

        if(Input.GetMouseButtonDown(0)) {                                                //버튼이 눌렸으면.
            this.click_timer = 0.0f;                                                    //타이머를 리셋
        } else {
            if(this.click_timer >= 0.0f) {                                              //그렇지 않으면
                this.click_timer += Time.deltaTime;                                     //경과 시간을 더한다
            }
        }

        //다음의 상태가 정해져 있지 않으면 상태의 변화를 조사한다
        if(next_step == STEP.NONE) {
            switch(step) {
                case STEP.RUN :
                    // if(! this.is_landed) {
                    //     //달리는 중이고 착지하지 않은 경우 아무것도 하지 않는다
                    // } else {
                    //     if(Input.GetMouseButtonDown(0)) {
                    //         //달리는 중이고 착지했고 왼쪽 버튼이 눌렸다면
                    //         //다음 상태를 점프로 변경
                    //         this.next_step = STEP.JUMP;
                    //     }
                    // }
                    
                    //click_timer가 0 이상, CLICK_GRACE_TIME 이하이고
                    if(0.0f <= this.click_timer && this.click_timer <= CLICK_GRACE_TIME) {
                        if(this.is_landed) {            //착지했다면
                            this.click_timer = -1.0f;   //버튼이 눌리지 않은 상태를 나타내는 -1.0f로
                            this.next_step = STEP.JUMP; //점프 상태로 한다
                        }
                    }
                    break;
                case STEP.JUMP :                                                    //점프 중일때
                    if(this.is_landed) {
                            //점프중이고 착지했다면 다음 상태를 주행 중으로 변경
                        this.next_step = STEP.RUN;
                    }
                    break;
            }
        }
        //'다음 정보' 가 '상태 정보 없음'이 아닌 동안(상태가 변할 때만)
        while(next_step != STEP.NONE) {
            step = next_step;                       //'현재 상태'를 '다음 상태'로 갱신
            next_step = STEP.NONE;                  //'다음 상태'를 '상태 없음'으로 변경
            switch(step) {                          //갱신된 '현재 상태'가
                case STEP.JUMP :                    //'점프'일 때
                    //점프할 높이로 점프 속도를 계산(공식임)
                    velocity.y = Mathf.Sqrt(
                        2.0f * 9.8f * PlayerController.JUMP_HEIGHT_MAX);
                        // '버튼이 떨어졌음을 나타내는 플래그'를 클리어한다
                        this.is_key_released = false;
                        break;
            }
            step_timer = 0.0f;                  //상태가 변했으므로 경과 시간을 제로로 리셋
        }

        switch(this.step) {
            case STEP.RUN : 
                //속도를 높인다
                velocity.x += PlayerController.ACCELERATION * Time.deltaTime;
                //속도가 최고 속도 제한을 넘으면
                // if(Mathf.Abs(velocity.x) > PlayerController.SPEED_MAX) {
                //     //최고 속도 제한 이하로 유지한다.
                //     velocity.x *= PlayerController.SPEED_MAX / Mathf.Abs(this.GetComponent<Rigidbody>().velocity.x);
                // }
                // break;

                //계산으로 구한 속도가 설정해야할 속도를 넘었다면
                if(Mathf.Abs(velocity.x) > this.current_speed) {
                    //넘지 않게 조정한다
                    velocity.x *= this.current_speed / Mathf.Abs(velocity.x);
                }
                break;

            case STEP.JUMP :                                        //점프 중일 때
                do {
                    //'버튼이 떨어진 순간'이 아니면
                    if(! Input.GetMouseButtonUp(0)) {
                        break;      //아무것도 하지 않고 루프를 빠져나간다
                    }
                    //이미 감속된 상태면(두번 이상 감속하지 않도록)
                    if(this.is_key_released) {
                        break;      //아무것도 하지 않고 루프를 빠져나간다
                    }
                    //상하 방향속도가 0 이하면(하강 중이라면)
                    if(velocity.y <=0.0f) {
                        break;              //아무것도 하지 않고 루프를 빠져나간다
                    }

                    //버튼이 떨어져 있고 상승 중이라면 감속 시작
                    //점프의 상승은 여기서 끝
                    velocity.y *= JUMP_KEY_RELEASE_REDUCE;
                    Debug.Log("jump속도 감속");
                    this.is_key_released = true;
                } while(false);
            break;

            case STEP.MISS : 
                //가속도(ACCELERATION)를 빼서 Player의 속도를 느리게 해 간다
                velocity.x -=PlayerController.ACCELERATION * Time.deltaTime;
                if(velocity.x <0.0f) {      //Player의 속도가 마이너스면
                    velocity.x = 0.0f;      //0으로 한다
                }
            break;        
        }

        //Rigidbody의 속도를 위에서 구한 속도로 갱신
        //(이 행은 상태에 관계없이 매번 실행된다)
        this.GetComponent<Rigidbody>().velocity = velocity;
        
        
    }

    private void check_landed() {
        this.is_landed = false;                             //일단 false로 설정
        do {
            Vector3 s = this.transform.position;            //Player 현재 위치
            Vector3 e = s + Vector3.down * 1.0f;            //s부터 아래로 1.0f로 이동한 위치
            RaycastHit hit;
            if(! Physics.Linecast(s, e, out hit)) {         //s부터 e 사이에 아무것도 없을 때
                 break;                                     //아무것도 하지않고 do~while루프를 빠져나감(탈출구로)
            } 

            //s부터 e 사이에 뭔가 있을 때 아래의 처리가 실행
            if(this.step == STEP.JUMP) {                    //현재, 점프 상태라면.
                //경과 시간이 3.0f 미만이라면
                if(this.step_timer < Time.deltaTime * 3.0f) {
                    break;                                  //아무것도 하지 않고 do~while 루프를 빠져나감(탈출구로)
                }
            }
            //s부터 e사이에 뭔가 있고 JUMP직후가 아닐 때만 아래가 실행
            this.is_landed = true;
        } while(false);
        //루프의 탈출구
    }

    public bool isPlayEnd() {
        bool ret = false;
        switch(this.step) {
            case STEP.MISS :            //MISS 상태라면
            ret = true;                 //'죽었어요'(true)라고 알려줌
            break;
        }
        return ret;
    }
}
