using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using user;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField idInputTMP;
    [SerializeField] private TMP_InputField pwInputTMP;
    [SerializeField] private GameObject idErrorMessage;
    [SerializeField] private GameObject pwErrorMessage;
    // 정규식 패턴
    private readonly string idPattern = @"^[a-z]+[0-9]+$"; // 영어 소문자 먼저, 그 다음 숫자
    private readonly string pwPattern = @"^(?=.*[a-z])(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).*$"; // 영어 소문자, 숫자, 기호 조합
    
    private void Start()
    {
        // 에러 메시지 초기 상태 설정
        if (idErrorMessage) idErrorMessage.SetActive(false);
        if (pwErrorMessage) pwErrorMessage.SetActive(false);
        
        if (pwInputTMP != null)
        {
        pwInputTMP.contentType = TMP_InputField.ContentType.Password;
        }
        // 입력 필드에 이벤트 리스너 추가
        idInputTMP.onValueChanged.AddListener(ValidateID);
        pwInputTMP.onValueChanged.AddListener(ValidatePW);
        // 최대 길이 설정
        idInputTMP.characterLimit = 10;
        pwInputTMP.characterLimit = 12;
    }
    
    
    private void ValidateID(string input)
    {
        // 입력이 비어있으면 검증하지 않음
        if (string.IsNullOrEmpty(input))
        {
            if (idErrorMessage) idErrorMessage.SetActive(false);
            return;
        }
        
        // 패턴 검증
        bool isValid = Regex.IsMatch(input, idPattern);
        
        // 길이 검증 (6~10자리)
        bool hasCorrectLength = input.Length >= 6 && input.Length <= 10;
        
        // 에러 메시지 표시/숨김
        if (idErrorMessage)
        {
            idErrorMessage.SetActive(!isValid || !hasCorrectLength);
        }
        
        // 색상 설정 - 청록색 값 수정
        if (isValid && hasCorrectLength)
        {
            // 청록색 값을 명시적으로 지정
            idInputTMP.textComponent.color = new Color32(75, 188, 171, 255); // #4BBCAB
        }
        else
        {
            idInputTMP.textComponent.color = new Color32(235, 87, 87, 255); // #EB5757
        }
    }
    
    private void ValidatePW(string input)
    {
        // 입력이 비어있으면 검증하지 않음
        if (string.IsNullOrEmpty(input))
        {
            if (pwErrorMessage) pwErrorMessage.SetActive(false);
            return;
        }
        
        // 패턴 검증
        bool isValid = Regex.IsMatch(input, pwPattern);
        
        // 길이 검증
        bool hasCorrectLength = input.Length >= 8 && input.Length <= 12;
        
        // 에러 메시지 표시/숨김
        if (pwErrorMessage)
        {
            pwErrorMessage.SetActive(!isValid || !hasCorrectLength);
        }
        
        // 색상 설정 - 청록색 값 수정
        if (isValid && hasCorrectLength)
        {
            // 청록색 값을 명시적으로 지정
            pwInputTMP.textComponent.color = new Color32(75, 188, 171, 255); // #4BBCAB
        }
        else
        {
            pwInputTMP.textComponent.color = new Color32(235, 87, 87, 255); // #EB5757
        }
    }
    
    
    public void OnLoginButtonClick()
    {
        string id = idInputTMP.text;
        string password = pwInputTMP.text;
        
        // ID 유효성 검사 (길이와 패턴 분리)
        bool isIdLengthValid = id.Length >= 6 && id.Length <= 10;
        bool isIdPatternValid = Regex.IsMatch(id, idPattern);
        bool isIdValid = isIdLengthValid && isIdPatternValid;
        
        // 비밀번호 유효성 검사 (길이와 패턴 분리)
        bool isPwLengthValid = password.Length >= 8 && password.Length <= 12;
        bool isPwPatternValid = Regex.IsMatch(password, pwPattern);
        bool isPwValid = isPwLengthValid && isPwPatternValid;
        
        if (isIdValid && isPwValid)
        {
            Debug.Log("로그인 시도: ID=" + id);
            using (var log_reader = dbManager.select("user", "*", $"user_id='{id}'"))
            {
                if (log_reader == null || !log_reader.HasRows)  // 데이터가 없는 경우 체크
                {
                    Debug.Log("로그인 실패: ID가 존재하지 않습니다.");
                    // 화면에 오류 메시지 출력
                }
                else
                {
                    while (log_reader.Read())
                    {
                        if (Convert.ToString(log_reader["user_pw"]) == password)
                        {
                            Debug.Log("로그인 성공!");
                            User.Instance.setId(id);
                            User.Instance.setPw(password);
                            User.Instance.setName(Convert.ToString(log_reader["username"]));

                            SceneManager.LoadScene("UserDetail");
                        }
                        else
                        {
                            Debug.Log("로그인 실패: 비밀번호가 틀립니다");
                            // 화면에 오류 메시지 출력
                        }
                    }
                }
            }
        }
        else
        {
            // ID 에러 메시지 처리
            if (idErrorMessage && !string.IsNullOrEmpty(id) && !isIdValid)
            {
                idErrorMessage.SetActive(true);
                StartCoroutine(FlashErrorMessage(idErrorMessage));
            }
            
            // 비밀번호 에러 메시지 처리
            if (pwErrorMessage && !string.IsNullOrEmpty(password) && !isPwValid)
            {
                pwErrorMessage.SetActive(true);
                StartCoroutine(FlashErrorMessage(pwErrorMessage));
            }
        }
    }
    
    // 에러 메시지를 1초 동안 빨간색으로 변경하는 코루틴
    private IEnumerator FlashErrorMessage(GameObject errorMessageObj)
    {
        // 에러 메시지의 텍스트 컴포넌트 가져오기
        TextMeshProUGUI errorText = errorMessageObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (errorText != null)
        {
            // 원래 색상 저장
            Color originalColor = errorText.color;
            
            // 빨간색으로 변경
            errorText.color = Color.red;
            
            // 1초 대기
            yield return new WaitForSeconds(1f);
            
            // 원래 색상으로 복원
            errorText.color = originalColor;
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 리스너 제거
        idInputTMP.onValueChanged.RemoveListener(ValidateID);
        pwInputTMP.onValueChanged.RemoveListener(ValidatePW);
    }

    // Update 메서드 추가
    private void Update()
    {
        // Tab 키를 눌렀을 때 다음 입력 필드로 이동
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 현재 선택된 UI 요소 확인
            if (idInputTMP.isFocused)
            {
                // ID 필드에서 Tab을 누르면 비밀번호 필드로 이동
                idInputTMP.DeactivateInputField();
                pwInputTMP.Select();
                pwInputTMP.ActivateInputField();
            }
            else if (pwInputTMP.isFocused && Input.GetKey(KeyCode.LeftShift))
            {
                // 비밀번호 필드에서 Shift+Tab을 누르면 ID 필드로 이동
                pwInputTMP.DeactivateInputField();
                idInputTMP.Select();
                idInputTMP.ActivateInputField();
            }
        }
    }
}