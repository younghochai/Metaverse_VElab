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
    {{0, "커피"},{1, "디카페인"},{2, "차"},{3, "스무디"},{4, "디저트"}};
    ////////////////////////////////////////////////////////////
    Dictionary<int, string> twoD_coffee = new  Dictionary<int, string>
    {{0, "아메리카노"},{1, "카페라떼"},{2, "바닐라라떼"},{3, "카라멜마끼아또"},{4, "콜드브루"}};
    Dictionary<int, string> twoD_decaf = new Dictionary<int, string>
    {{0, "디카페인_아메리카노"},{1, "디카페인_카페라떼"},{2, "디카페인_바닐라라떼"},{3, "디카페인_카라멜마끼아또"},{4, "디카페인_콜드브루"}};
    Dictionary<int, string> twoD_tea = new Dictionary<int, string>
    {{0, "얼그레이티"},{1, "루이보스티"},{2, "쟈스민티"},{3, "캐모마일"},{4, "히비스커스"}};
    Dictionary<int, string> twoD_smoothy = new Dictionary<int, string>
    {{0, "플레인요거트_스무디"},{1, "망고_스무디"},{2, "딸기요거트_스무디"},{3, "블루베리_스무디"},{4, "바닐라_스무디"}};
    Dictionary<int, string> twoD_dessert = new Dictionary<int, string>
    {{0, "치즈케이크"},{1, "티라미수"},{2, "마카롱"},{3, "쿠키"},{4, "다쿠아즈"}};
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


    int MenuIndex = 0;
    int change_counter = 0;

    public string gesture_direction = "";
    string selected_menu = "";
    string current_menu;
    bool is_step0, is_step1, is_step2, is_step3, is_step4 = false;
    bool is_duplicate = false;
    bool is_ready;
    Renderer kioskIMG;
    Material start;
    Material C1, C2, C3, C4, C5;
    //Material C1_1, C1_2, C1_3, C1_4, C1_5;
    //Material C2_1, C2_2, C2_3, C2_4, C2_5;
    //Material C3_1, C3_2, C3_3, C3_4, C3_5;
    //Material C4_1, C4_2, C4_3, C4_4, C4_5;
    //Material C5_1, C5_2, C5_3, C5_4, C5_5;
    //for test.


    void step1_2_SELECT_CATEGORY(string direction, Dictionary<int, string> category_OR_menu, int step_num) 
    {
        Material[] MenuMaterial = new Material[] { C1, C2, C3, C4, C5 };
        kioskIMG.material = MenuMaterial[MenuIndex];
        if (!is_duplicate) 
        {
            if (direction == "Up")
            {
                Debug.Log("선택. 해당 메뉴로 이동합니다");
                change_counter = 0;
                selected_menu = current_menu;
                MenuIndex = 0;
                if (step_num == 1) { is_step1 = false; is_step2 = true; }
                if (step_num == 2) { is_step2 = false; is_step3 = true; }

                is_duplicate = true;
            }
            if (direction == "Down")
            {
                Debug.Log("취소. 이전 메뉴로 이동합니다.");
                change_counter = 0;
                MenuIndex = 0;
                if (step_num == 1) { is_step1 = false; is_step0 = true; }
                if (step_num == 2) { is_step2 = false; is_step1 = true; }
                is_duplicate = true;
            }
            if (direction == "Left")
            {
                Debug.Log("다음메뉴");
                MenuIndex++;
                change_counter = 0;
                if (MenuIndex > 4) MenuIndex = 0;
                is_duplicate = true;
            }
            if (direction == "Right")
            {
                Debug.Log("이전메뉴");
                MenuIndex--;
                change_counter = 0;
                if (MenuIndex < 0) MenuIndex = 4;
                is_duplicate = true;
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////
        if (direction == "Ready") is_duplicate = false;
        if (change_counter == 0 && direction != "Down" && direction != "Up")
        {
            current_menu = category_OR_menu[MenuIndex];
            Debug.LogFormat("현재 선택된 메뉴는 [{0}]입니다.", current_menu);
            change_counter++;
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {

        C1 = Resources.Load<Material>("1_Coffee");
        C2 = Resources.Load<Material>("2_Decaf");
        C3 = Resources.Load<Material>("3_Tea");
        C4 = Resources.Load<Material>("4_Smoothy");
        C5 = Resources.Load<Material>("5_Dessert");

        start = Resources.Load<Material>("Materials/KioskMenuMaterial/Ready");
        //Debug.Log("어서오세요. 원하는 카테고리를 선택해주세요");
        is_step0 = true;

    }

    // Update is called once per frame
    void Update()
    {
        kioskIMG = GameObject.Find("Screen").GetComponent<MeshRenderer>();
        gesture_direction = GameObject.Find("Xsens").GetComponent<motion_gesture>().direction;
        is_ready = GameObject.Find("Xsens").GetComponent<motion_gesture>().is_ready_to_order;

        if (is_step0) 
        {
            //Debug.Log("키오스크의 시작입니다. 어서오세요!. 시작하시려면 좌우로 손을 흔들어주세요.");
            kioskIMG.material = start;

            if (is_ready) 
            {
                is_step0 = false;
                is_step1 = true;
            }

        }
        if (is_step1) 
        {
            C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/1_Coffee");
            C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/2_Decaf");
            C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/3_Tea");
            C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/4_Smoothy");
            C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/1_Category/5_Dessert");
            step1_2_SELECT_CATEGORY(gesture_direction, oneD_Category, 1);
        }
        if (is_step2) 
        {
            if (selected_menu == "커피")
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_1");C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_3");C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/2_Coffee/Coffee_5");
                step1_2_SELECT_CATEGORY(gesture_direction, twoD_coffee, 2);
            }
            if (selected_menu == "디카페인")
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/3_Decaf/Decaf_5");
                step1_2_SELECT_CATEGORY(gesture_direction, twoD_decaf, 2); 
            }

            if (selected_menu == "차") 
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/4_Tea/Tea_5");
                step1_2_SELECT_CATEGORY(gesture_direction, twoD_tea, 2); 
            }
            if (selected_menu == "스무디") 
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/5_Smoothy/Smoothy_5");
                step1_2_SELECT_CATEGORY(gesture_direction, twoD_smoothy, 2); 
            }
            if (selected_menu == "디저트") 
            {
                C1 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_1"); C2 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_2");
                C3 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_3"); C4 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_4");
                C5 = Resources.Load<Material>("Materials/KioskMenuMaterial/6_Dessert/Dessert_5");
                step1_2_SELECT_CATEGORY(gesture_direction, twoD_dessert, 2); 
            }
        }
        if (is_step3) 
        {
            //핫 OR 아이스? 
            //메뉴를 추가 주문하시겠습니까?(YES: 카테고리로 이동, NO: 결제창으로 이동)

            //Debug.LogFormat("선택한 메뉴의 가격은[{}원입니다.]", Total_Menu_price[selected_menu]);
            //Debug.LogFormat("선택한 메뉴는 [{}]입니다.]", selected_menu);
            
            Debug.Log("구현 준비중...");
            selected_menu = "커피";

            is_step2 = true;
            is_step3 = false;
        }
        if (is_step4) 
        {
            //결제 수단을 선택해주세요...
        }

    }
}
