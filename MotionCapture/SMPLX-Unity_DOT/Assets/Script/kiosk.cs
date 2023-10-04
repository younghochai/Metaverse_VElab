using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public class kiosk : MonoBehaviour
{
    Dictionary<int, string> oneD_Category = new Dictionary<int, string>
    {{0, "커피"},{1, "디카페인"},{2, "차"},{3, "스무디"},{4, "디저트"},{5, "장바구니"} };
    ////////////////////////////////////////////////////////////
    Dictionary<int, string> twoD_coffee = new  Dictionary<int, string>
    {{0, "아메리카노"},{1, "카페라떼"},{2, "바닐라라떼"},{3, "카라멜마끼아또"},{4, "콜드브루"},{5, "장바구니"}};
    Dictionary<int, string> twoD_decaf = new Dictionary<int, string>
    {{0, "디카페인_아메리카노"},{1, "디카페인_카페라떼"},{2, "디카페인_바닐라라떼"},{3, "디카페인_카라멜마끼아또"},{4, "디카페인_콜드브루"},{5, "장바구니"}};
    Dictionary<int, string> twoD_tea = new Dictionary<int, string>
    {{0, "얼그레이티"},{1, "루이보스티"},{2, "쟈스민티"},{3, "캐모마일"},{4, "히비스커스"},{5, "장바구니"}};
    Dictionary<int, string> twoD_smoothy = new Dictionary<int, string>
    {{0, "플레인요거트_스무디"},{1, "망고_스무디"},{2, "딸기요거트_스무디"},{3, "블루베리_스무디"},{4, "바닐라_스무디"},{5, "장바구니"}};
    Dictionary<int, string> twoD_dessert = new Dictionary<int, string>
    {{0, "치즈케이크"},{1, "티라미수"},{2, "마카롱"},{3, "쿠키"},{4, "다쿠아즈"},{5, "장바구니"}};
    /// /////////////////////////////////////////////////////////

    Dictionary<int, string> threeD_pay = new Dictionary<int, string>
    {{0, "신용카드"},{1, "네이뵤페이"},{2, "캬캬오페이"},{3, "쿠폰사용"}};

    Dictionary<string, int> Total_Menu_price = new Dictionary<string, int>
    {
        {"아메리카노", 3000},{"카페라떼", 3500},{"바닐라라떼", 4000},{"카라멜마끼아또",4500},{"콜드브루",4000},
        {"디카페인_아메리카노", 3300},{"디카페인_카페라떼", 3800},{"디카페인_바닐라라떼", 4300},{"디카페인_카라멜마끼아또",4800},{"디카페인_콜드브루",4300},
        {"얼그레이티", 2800},{"루이보스티", 2800},{"쟈스민티", 2800},{"캐모마일",2800},{"히비스커스",2800},
        {"플레인요거트_스무디", 4500},{"망고_스무디", 4500},{"딸기요거트_스무디", 2800},{"블루베리_스무디",2800},{"바닐라_스무디",2800},
        {"치즈케이크", 4500},{"티라미수", 5000},{"마카롱", 3000},{"쿠키",2500},{"다쿠아즈",3000}
    };
    Dictionary<string, int> cart  = new Dictionary<string, int>();
    Dictionary<int, string> cart_modify = new Dictionary<int, string>();


    int MenuIndex = 0;
    int change_counter = 0;
    int total_sum_price = 0;
    int prior_depth = 0;

    public string direction_from_motion_gesture = "";
    string kiosk_direction = "";
    string selected_category = "";
    string selected_menu = "";
    string current_menu;

    bool is_step0, is_step1, is_step2, is_step3, is_step4, is_step5, is_cart_modify = false;
    bool is_duplicate = false;
    bool is_ready = false;
    bool is_played = false;

    Renderer kioskIMG;
    Material start;
    Material C1, C2, C3, C4, C5, C6, C7;
    public Text printMessage_1, printMessage_2;

    float waitingTime;
    float timer;

    void step1_2_SELECT_CATEGORY(string direction, Dictionary<int, string> category_OR_menu, int step_num) 
    {
        Material[] MenuMaterial = new Material[] { C1, C2, C3, C4, C5, C6 };
        kioskIMG.material = MenuMaterial[MenuIndex];
        if (!is_duplicate) 
        {
            if (direction == "Up")
            {
                change_counter = 0;
                MenuIndex = 0;
                Debug.Log("선택. 해당 메뉴로 이동합니다");
                Debug.LogFormat("이때의 선택된 메뉴는?: {0}", current_menu);


                //current_menu가  cart인 상태로 up이 눌렸을 경우
                if (current_menu == "장바구니")//1뎁스나 2뎁스에서 장바구니로 바로 갑니다. 만약에 돌아 올 때는 prior 변수로 다시 돌아옵니다.
                {
                    Debug.Log("카트 분기 진입 체크용.");

                    is_step1 = false; is_step2 = false;
                    is_step4 = true;
                }

                if (step_num == 1 && current_menu != "장바구니") //1뎁스에서 2뎁스로 넘어갈 때
                { 
                    is_step1 = false; is_step2 = true;
                    selected_category = current_menu;

                }
                //메뉴를 선택하고, 장바구니에 추가한 후 뎁스3으로 넘어갑니다. 
                if (step_num == 2 && current_menu != "장바구니") 
                {
                    selected_menu = current_menu;

                    //장바구니에 추가
                    if (cart.ContainsKey(current_menu))
                    {
                        cart[current_menu] += 1;
                    }
                    else
                    {
                        cart.Add(current_menu, 1);
                    }

                    is_step2 = false; is_step3 = true; 
                }

                is_duplicate = true;
                direction_from_motion_gesture = "Defualt";
            }
            if (direction == "Down")
            {
                Debug.Log("취소. 이전 메뉴로 이동합니다.");
                change_counter = 0;
                MenuIndex = 0;
                if (step_num == 1) { is_step1 = false; is_step0 = true; }
                if (step_num == 2) { is_step2 = false; is_step1 = true; }
                is_duplicate = true;
                direction_from_motion_gesture = "Defualt";
            }
            if (direction == "Left")
            {
                Debug.Log("다음메뉴");
                MenuIndex++;
                change_counter = 0;
                if (MenuIndex > 5) MenuIndex = 0;
                is_duplicate = true;
                direction_from_motion_gesture = "Defualt";
            }
            if (direction == "Right")
            {
                Debug.Log("이전메뉴");
                MenuIndex--;
                change_counter = 0;
                if (MenuIndex < 0) MenuIndex = 5;
                is_duplicate = true;
                direction_from_motion_gesture = "Defualt";
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////
        if (direction == "Defualt") is_duplicate = false;
        if (change_counter == 0 && direction != "Down" && direction != "Up")
        {
            current_menu = category_OR_menu[MenuIndex];
            Debug.LogFormat("현재 선택된 메뉴는 [{0}]입니다.", current_menu);
            change_counter++;
        }
        
    }
    void step1_2_SELECT_CATEGORY1(string direction, Dictionary<int, string> category_OR_menu, int step_num)
    {
        Material[] MenuMaterial = new Material[] { C1, C2, C3, C4, C5, C6 };
        kioskIMG.material = MenuMaterial[MenuIndex];

        if (!is_played) //최초 실행시 한번만 처리합니다. or 디렉션이 입력되어 갱신될 경우 한번 플레이합...니까?
        {
            //현재의 메뉴를 보여주기.
            current_menu = category_OR_menu[MenuIndex];
            Debug.LogFormat("현재 선택된 메뉴는 [{0}]입니다.", current_menu);
            is_played = true;
        }
        if (is_played) 
        {
            if (direction == "Up")
            {
                MenuIndex = 0;
                Debug.Log("선택. 해당 메뉴로 이동합니다");
                Debug.LogFormat("이때의 선택된 메뉴는?: {0}", current_menu);

                //current_menu가  cart인 상태로 up이 눌렸을 경우
                if (current_menu == "장바구니")//1뎁스나 2뎁스에서 장바구니로 바로 갑니다. 만약에 돌아 올 때는 prior 변수로 다시 돌아옵니다.
                {
                    is_step1 = false; is_step2 = false;
                    is_step4 = true;
                }

                if (step_num == 1 && current_menu != "장바구니") //1뎁스에서 2뎁스로 넘어갈 때
                {
                    is_step1 = false; is_step2 = true;
                    selected_category = current_menu;

                }
                //메뉴를 선택하고, 장바구니에 추가한 후 뎁스3으로 넘어갑니다. 
                if (step_num == 2 && current_menu != "장바구니")
                {
                    selected_menu = current_menu;

                    //장바구니에 추가
                    if (cart.ContainsKey(current_menu))
                    {
                        cart[current_menu] += 1;
                    }
                    else
                    {
                        cart.Add(current_menu, 1);
                    }
                    is_step2 = false; is_step3 = true;
                }
                direction_from_motion_gesture = "Defualt";
                is_played = false;
            }
            if (direction == "Down")
            {
                Debug.Log("취소. 이전 메뉴로 이동합니다.");
                MenuIndex = 0;
                if (step_num == 1) { is_step1 = false; is_step0 = true; }
                if (step_num == 2) { is_step2 = false; is_step1 = true; }
                is_duplicate = true;
                direction_from_motion_gesture = "Defualt";
                is_played = false;
            }
            if (direction == "Left")
            {
                Debug.Log("다음메뉴");
                MenuIndex++;
                if (MenuIndex > 5) MenuIndex = 0;
                direction_from_motion_gesture = "Defualt";
                is_played = false;
            }
            if (direction == "Right")
            {
                Debug.Log("이전메뉴");
                MenuIndex--;
                if (MenuIndex < 0) MenuIndex = 5;
                direction_from_motion_gesture = "Defualt";
                is_played = false;
            }
        }
    }
    void in_cart()
    {
        if (!is_played) 
        {
            Debug.Log("장바구니 분기 진입.");
            printMessage_1.text = "현재 단계는 '장바구니 단계'입니다.";
            //printMessage_2.text = "위: 결제수단으로 넘어가기\n아래: 이전 메뉴로 돌아가기\n왼쪽: 장바구니 수정.";
            //1. 현재 장바구니에 있는 메뉴들을 불러줍니다. 
            //만약 딕셔너리가 비었다면 없다고 출력하고 초기 메뉴로 넘어갑니다.
            if (cart.Keys.Count == 0)
            {
                Debug.Log("현재 장바구니가 비어있습니다. 주문을 위해 초기 메뉴로 이동합니다.");
                is_step0 = false; is_step2 = false; is_step3 = false; is_step4 = false;
                is_step1 = true;

                change_counter = 0;
                MenuIndex = 0;
            }
            else
            {
                Debug.Log("현재 장바구니에 있는 메뉴들은 다음과 같습니다.");

                //메뉴의 갯수가 0개인 키가 있으면 그걸 삭제합니다.
                List<string> remove_keys_list = new List<string>();
                foreach (var key in cart.Keys)
                {
                    if (cart[key] == 0) remove_keys_list.Add(key);
                }
                foreach (var item in remove_keys_list) { cart.Remove(item); }
                //메뉴의 갯수가 0개인 키가 있으면 그걸 삭제합니다.


                if (cart.Keys.Count != 0) //혹시 모든 메뉴가 삭제당하지 않았는지 확인을 합니다. 만약 모든 메뉴가 삭제당했다면 밑에서 초기로 넘어갑니다.
                {
                    foreach (var key in cart.Keys)
                    {
                        Debug.LogFormat("{0} : {1}개", key, cart[key]);
                    }
                }
            }
            if (cart.Keys.Count == 0)
            {
                Debug.Log("현재 장바구니가 비어있습니다. 주문을 위해 초기 메뉴로 이동합니다.");
                is_step0 = false; is_step2 = false; is_step3 = false; is_step4 = false;
                is_step1 = true;

                change_counter = 0;
                MenuIndex = 0;
            }

            //2. 그 후 그 메뉴와 수량에 대한 총 가격을 출력합니다.
            if (cart.Keys.Count != 0)
            {
                total_sum_price = 0; // 다른 분기에 들어왔을 때 겹치지 않게 초기화...
                foreach (var key in cart.Keys)
                {
                    total_sum_price += Total_Menu_price[key] * cart[key];
                }
                Debug.LogFormat("결제 총 금액: {0}", total_sum_price);
                Debug.Log("진행을 위해 제스처를 취해주세요. 위: 결제로 진행. 아래: 직전메뉴.");
                is_played = true;
            }
        }
        if (is_played) //장바구니 리스트 한번 플레이 되고. 선택지를 넣는 곳.
        {
            //Debug.Log("장바구니: 제스처 분기 진입.");

            if (direction_from_motion_gesture == "Up")
            {
                Debug.Log("장바구니_위쪽 \n확인. 결제 단계로 넘어갑니다.");

                is_played = false; is_step4 = false; is_step5 = true;
                direction_from_motion_gesture = "Defualt";
            }
            if (direction_from_motion_gesture == "Down")
            {
                Debug.Log("장바구니_아래쪽 \n.직전 메뉴로 돌아갑니다.");
                is_played = false; is_step4 = false;
                if (prior_depth == 1) { is_step1 = true; }
                if (prior_depth == 2) { is_step2 = true; Debug.LogFormat("이때의 selected_menu: {0}", selected_menu);
                }
                if (prior_depth == 3) { is_step2 = true; Debug.LogFormat("이때의 selected_menu: {0}", selected_menu);
                }
                direction_from_motion_gesture = "Defualt";
            }
            if (direction_from_motion_gesture == "Left")//메뉴 수정(수량 증가 or 감소(0이면 삭제))
            {
                Debug.Log("장바구니_왼쪽.초기 메뉴로 돌아갑니다.");
                is_played = false; //is_step4 = false; is_step5 = true;
                is_cart_modify = true;
                direction_from_motion_gesture = "Defualt";

            }
            if (direction_from_motion_gesture == "Right")
            {
                Debug.Log("장바구니_오른쪽.초기 메뉴로 돌아갑니다.");
                is_played = false; is_step4 = false; is_step5 = true;
                direction_from_motion_gesture = "Defualt";

            }
        }
    }
    void cart_menu_modifier()
    // 0번 메뉴를 따로 추가하자. 0번에서 뒤로가기, 앞으로 가기 할 경우, 다시 장바구니 뎁스로.
    // 실제 메뉴에서는 그 메뉴에서 좌 우에 따라 수량을 수정하게 함.

    {
        if (!is_played)// 한번 실행되었을 때
        {
            printMessage_1.text = "현재 단계는 '장바구니 수정 단계'입니다. 이전 메뉴로 돌아가려면 메뉴선택 항목에서 위 아래를 입력해주세요.";
            //printMessage_2.text = "위: 수량 증가/돌아가기\n아래: 수량감소/돌아가기\n왼쪽,오른쪽: 메뉴이동";
            cart_modify = new Dictionary<int, string>(); //들어올 때마다 카트 수정값 초기화 합니다. 안그러면 중복 오류 뜸
            MenuIndex = 0;
            int i = 1;
            cart_modify.Add( 0, "선택메뉴");
            foreach (var key in cart.Keys) 
            {
                cart_modify.Add(i, key);
                i++;
            }
            i = 0;
            foreach (var key in cart_modify.Keys) 
            {
                Debug.LogFormat("수정된 장바구니 현황:\n{0}: {1}",key, cart_modify[key]);
            }
            is_played = true;
            // 장바구니에서 수량을 변경할 메뉴를 골라주세요.
        }
        if (is_played)
        {
            if (direction_from_motion_gesture == "Up") //수량 업
            {
                if (MenuIndex == 0)
                {
                    Debug.Log("장바구니 수정_위쪽.수정끝났습니다.");
                    is_cart_modify = false;
                    is_played = false;

                }
                else 
                {
                    Debug.LogFormat("{0}의 메뉴 수량을 늘립니다.", cart_modify[MenuIndex]);

                    cart[cart_modify[MenuIndex]] += 1;
                    Debug.LogFormat("{0}의 수량: {1}", cart_modify[MenuIndex], cart[cart_modify[MenuIndex]]);

                }
                direction_from_motion_gesture = "Defualt";

            }
            if (direction_from_motion_gesture == "Down") //수량 다운
            {
                if (MenuIndex == 0)
                {
                    Debug.Log("장바구니 수정_아래쪽.수정끝났습니다.");
                    is_cart_modify = false;
                    is_played = false;
                }
                else 
                {
                    Debug.LogFormat("{0}의 메뉴 수량을 줄입니다.", cart_modify[MenuIndex]);

                    cart[cart_modify[MenuIndex]] -= 1;
                    if (cart[cart_modify[MenuIndex]] < 0) cart[cart_modify[MenuIndex]] = 0;
                    Debug.LogFormat("{0}의 수량: {1}", cart_modify[MenuIndex], cart[cart_modify[MenuIndex]]);
                }


                direction_from_motion_gesture = "Defualt";

            }
            if (direction_from_motion_gesture == "Left") //메뉴 이동-좌
            {
                Debug.Log("장바구니 수정_왼쪽.수정끝났습니다.");
                MenuIndex++;
                if (MenuIndex > cart_modify.Keys.Count-1) MenuIndex = 0;

                if (MenuIndex == 0) Debug.LogFormat("메뉴: 장바구니로 돌아가려면 위나 아래쪽을 입력해주세요.");
                else 
                {
                    Debug.LogFormat("현재 선택된 장바구니 메뉴:{0}\n현재 선택된 메뉴의 수량:{1}", cart_modify[MenuIndex], cart[cart_modify[MenuIndex]]);
                }

                direction_from_motion_gesture = "Defualt";

            }
            if (direction_from_motion_gesture == "Right") //메뉴 이동-우
            {
                Debug.Log("장바구니 수정_오른쪽.수정끝났습니다.");
                MenuIndex--;
                if (MenuIndex < 0) MenuIndex = cart_modify.Keys.Count - 1;

                if (MenuIndex == 0) Debug.LogFormat("메뉴: 장바구니로 돌아가려면 위나 아래쪽을 입력해주세요.");
                else 
                {
                    Debug.LogFormat("현재 선택된 장바구니 메뉴:{0}\n현재 선택된 메뉴의 수량:{1}", cart_modify[MenuIndex], cart[cart_modify[MenuIndex]]);
                }

                direction_from_motion_gesture = "Defualt";

            }

            //좌우
        }// is played 초기화 한번 시켜야함
    }

    void PAYMENT() 
    {
        Material[] MenuMaterial = new Material[] { C1, C2, C3, C4};
        kioskIMG.material = MenuMaterial[MenuIndex];
        if (!is_played) // 처음만 실행됩니다.
        {        
            Debug.Log("결제모듈진입.");

            current_menu = threeD_pay[0];
            MenuIndex = 0;
            printMessage_1.text = "총 결제할 금액은 " + total_sum_price.ToString() + "원입니다.";
            //printMessage_2.text = "위: 선택한 결제수단으로 결제\n아래:장바구니로 돌아가기\n왼쪽,오른쪽:결제 수단 변경";
            Debug.LogFormat("총 결제할 금액은 {0}원입니다.", total_sum_price.ToString());
            Debug.Log("결제수단을 선택해주세요. 장바구니로 가시려면 뒤로가기를 눌러주세요.");
            Debug.LogFormat("선택된 결제수단은 '{0}' 입니다.", current_menu);

            //Debug.LogFormat("현재 메뉴는 '{0}' 입니다.", current_menu);

            is_played = true;
        }

        if (is_played) //이후에 대기하면서 돌아가는 스크립트입니다.
        {
            if (direction_from_motion_gesture == "Up")
            {
                Debug.Log("결제_위쪽.해당 결제 방식으로 결제를 진행해 주세요.");
                cart = new Dictionary<string, int>(); //장바구니 비우기 >> 초기화면으로 가기 위해...
                is_played = false; is_step5 = false;
                is_ready = false; is_step0 = true; //초기로 돌아갑니다.
                direction_from_motion_gesture = "Defualt";
            }
            if (direction_from_motion_gesture == "Down")
            {
                Debug.Log("결제_아래쪽.장바구니로 돌아갑니다.");
                is_played = false; is_step5 = false; is_step4 = true;
                direction_from_motion_gesture = "Defualt";

            }
            if (direction_from_motion_gesture == "Left")//메뉴 수정(수량 증가 or 감소(0이면 삭제))
            {
                Debug.Log("결제_왼쪽.");
                MenuIndex++;
                if (MenuIndex > 3) MenuIndex = 0;
                current_menu = threeD_pay[MenuIndex];
                Debug.LogFormat("선택된 결제수단은 '{0}' 입니다.", current_menu);
                direction_from_motion_gesture = "Defualt";
            }
            if (direction_from_motion_gesture == "Right")
            {
                Debug.Log("결제_오른쪽.");
                MenuIndex--;
                if (MenuIndex < 0) MenuIndex = 3;
                current_menu = threeD_pay[MenuIndex];
                Debug.LogFormat("선택된 결제수단은 '{0}' 입니다.", current_menu);

                direction_from_motion_gesture = "Defualt";
            }

        }


    }
    void COMMAND_WITH_ARROWS()
    {
        //direction_from_motion_gesture = "Defualt"; //자켓으로 컨트롤할때는여기 비활성화 시켜야함
        //화살표키로 디렉션 값 주기.
        if (Input.GetKeyDown(KeyCode.R)) { is_ready = true; }
        if (Input.GetKeyDown(KeyCode.UpArrow)) { direction_from_motion_gesture = "Up";  }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { direction_from_motion_gesture = "Down"; }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { direction_from_motion_gesture = "Left";}
        if (Input.GetKeyDown(KeyCode.RightArrow)) { direction_from_motion_gesture = "Right";  }
    }
    // Start is called before the first frame update
    void Start()
    {
        printMessage_1 = GameObject.Find("print_msg_3").GetComponent<Text>();
        printMessage_2 = GameObject.Find("print_msg_2").GetComponent<Text>();

        C1 = Resources.Load<Material>("1_Coffee");
        C2 = Resources.Load<Material>("2_Decaf");
        C3 = Resources.Load<Material>("3_Tea");
        C4 = Resources.Load<Material>("4_Smoothy");
        C5 = Resources.Load<Material>("5_Dessert");
        C6 = Resources.Load<Material>("Materials/KioskMenuMaterial/Cart");
        C7 = Resources.Load<Material>("Materials/KioskMenuMaterial/Cart_modify");
        start = Resources.Load<Material>("Materials/KioskMenuMaterial/Ready");
        //Debug.Log("어서오세요. 원하는 카테고리를 선택해주세요");
        is_step0 = true;

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //COMMAND_WITH_ARROWS(); //키보드 입력 모드
        printMessage_2.text = "현재 direction 입력값:" + direction_from_motion_gesture;


        kioskIMG = GameObject.Find("Screen").GetComponent<MeshRenderer>();
        //화살표로 컨트롤하는 모드일땐 여기 끄고 하기. 
        direction_from_motion_gesture = GameObject.Find("Xsens").GetComponent<motion_gesture>().direction;
        is_ready = GameObject.Find("Xsens").GetComponent<motion_gesture>().is_ready_to_order;

        // 1초의 입력 딜레이 텀을 준다. 한번 동작이 입력되면, 1초 후 다음 동작이 입력될 때까지 입력 비활성화
        if (direction_from_motion_gesture == "Up" || direction_from_motion_gesture == "Down" || direction_from_motion_gesture == "Left" || direction_from_motion_gesture == "Right")
        {
            if(timer >1.0)
            kiosk_direction = direction_from_motion_gesture;

        }
        if (is_step0)
        {
            if (!is_played) // 처음만 실행됩니다.
            {
                prior_depth = 0;
                MenuIndex = 0;
                printMessage_1.text = "키오스크의 시작입니다. 어서오세요!. \n시작하시려면 좌우로 손을 흔들어주세요.";
                Debug.Log("키오스크의 시작입니다. 어서오세요!. 시작하시려면 좌우로 손을 흔들어주세요.");
                kioskIMG.material = start;
                is_played = true;

            }

            if (is_played) //이후에 대기하면서 돌아가는 스크립트입니다.
            {
                
                if (is_ready)
                {
                    is_played = false;

                    is_step0 = false;
                    is_step1 = true;
                    
                }

            }

        }
        if (is_step1)
        {
            //printMessage_1.text = "주문하실 메뉴의 카테고리를 골라주세요! 다음 카테고리는 []입니다.";
            //printMessage_2.text = "위: 카테고리 선택, 아래:이전 메뉴\n왼쪽, 오른쪽: 메뉴 이동";
            prior_depth = 1;
            C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/1_Coffee");
            C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/2_Decaf");
            C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/3_Tea");
            C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/4_Smoothy");
            C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/5_Dessert");
            step1_2_SELECT_CATEGORY1(direction_from_motion_gesture, oneD_Category, 1);
        }
        if (is_step2)
        {
            prior_depth = 2;
            //printMessage_1.text = "주문하실 메뉴를 골라주세요!";
            //printMessage_2.text = "위: 카테고리 선택, 아래:이전 메뉴\n왼쪽, 오른쪽: 메뉴 이동";

            if (selected_category == "커피")
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_5"); C6 = Resources.Load<Material>("Materials/KioskMenuMaterial/Cart");
                step1_2_SELECT_CATEGORY1(direction_from_motion_gesture, twoD_coffee, 2);
            }
            if (selected_category == "디카페인")
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_5"); C6 = Resources.Load<Material>("Materials/KioskMenuMaterial/Cart");
                step1_2_SELECT_CATEGORY1(direction_from_motion_gesture, twoD_decaf, 2);
            }

            if (selected_category == "차")
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_5"); C6 = Resources.Load<Material>("Materials/KioskMenuMaterial/Cart");
                step1_2_SELECT_CATEGORY1(direction_from_motion_gesture, twoD_tea, 2);
            }
            if (selected_category == "스무디")
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_5"); C6 = Resources.Load<Material>("Materials/KioskMenuMaterial/Cart");
                step1_2_SELECT_CATEGORY1(direction_from_motion_gesture, twoD_smoothy, 2);
            }
            if (selected_category == "디저트")
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_5"); C6 = Resources.Load<Material>("Materials/KioskMenuMaterial/Cart");
                step1_2_SELECT_CATEGORY1(direction_from_motion_gesture, twoD_dessert, 2);
            }
            //if (selected_menu == "장바구니") { is_step2 = false; is_step4 = true; }
        }
        if (is_step3)
        {
            prior_depth = 3;

            //1. 핫 OR 아이스? >> 그런거 선택하지 말자...

            //2. 몇 잔?
            //3. 메뉴를 추가 주문하시겠습니까?(YES: 카테고리로 이동, NO: 장바구니로 이동)



            Debug.Log("추가메뉴를 시키겠습니까? 아니면 장바구니로 가겠습니까? 어차피 구현 안됐으니 장바구니로 갑시다.");//이거 구현해야함. til 개천절


 
            is_step3 = false;
            is_step4 = true;
            //is_played = false;
        }
        if (is_step4)
        {
            //장바구니 뎁스. 여기는 1뎁스든 2뎁스든 맘대로 넘어올 수 있습니다.
            if (!is_cart_modify) 
            {
                kioskIMG.material = C6;
                in_cart();
            }
            if (is_cart_modify) 
            {
                kioskIMG.material = C7;
                cart_menu_modifier();
            }
            
   


        }
        if (is_step5)
        {
            C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/7_Pay/Pay_Creditcard"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/7_Pay/Pay_Naverpay");
            C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/7_Pay/Pay_Kakaopay"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/7_Pay/Pay_Coupon");
            //결제 수단을 선택해주세요...
            PAYMENT();

        }

       

    }
}
