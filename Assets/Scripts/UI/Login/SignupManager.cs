using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using user;

public class SignupManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputTMP;
    [SerializeField] private TMP_InputField idInputTMP;
    [SerializeField] private TMP_InputField pwInputTMP;
    [SerializeField] private TMP_InputField pwConfirmInputTMP;
    
    [SerializeField] private GameObject nameErrorMessage;
    [SerializeField] private GameObject idErrorMessage;
    [SerializeField] private GameObject pwErrorMessage;
    [SerializeField] private GameObject pwConfirmErrorMessage;
    
    // 정규식 패턴
    private readonly string namePattern = @"^[가-힣]+$";
    private readonly string idPattern = @"^[a-z]+[0-9]+$"; // 영어 소문자 먼저, 그 다음 숫자
    private readonly string pwPattern = @"^(?=.*[a-z])(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).*$"; // 영어 소문자, 숫자, 기호 조합
    
    // 색상 정의
    private readonly Color validColor = new Color32(75, 188, 171, 255); // #4BBCAB - 청록색
    private readonly Color errorColor = new Color32(235, 87, 87, 255);  // #EB5757 - 부드러운 붉은색
    
    private void Start()
    {
        // 에러 메시지 초기 상태 설정
        if (nameErrorMessage) nameErrorMessage.SetActive(false);
        if (idErrorMessage) idErrorMessage.SetActive(false);
        if (pwErrorMessage) pwErrorMessage.SetActive(false);
        if (pwConfirmErrorMessage) pwConfirmErrorMessage.SetActive(false);
        
        // 비밀번호 필드 설정
        if (pwInputTMP != null)
        {
            pwInputTMP.contentType = TMP_InputField.ContentType.Password;
        }
        
        // 비밀번호 확인 필드 설정
        if (pwConfirmInputTMP != null)
        {
            pwConfirmInputTMP.contentType = TMP_InputField.ContentType.Password;
        }
        
        // 입력 필드에 이벤트 리스너 추가
        nameInputTMP.onValueChanged.AddListener(ValidateName);
        idInputTMP.onValueChanged.AddListener(ValidateID);
        pwInputTMP.onValueChanged.AddListener(ValidatePW);
        pwConfirmInputTMP.onValueChanged.AddListener(ValidatePWConfirm);
        
        // 최대 길이 설정
        nameInputTMP.characterLimit = 10;
        idInputTMP.characterLimit = 10;
        pwInputTMP.characterLimit = 12;
        pwConfirmInputTMP.characterLimit = 12;
        
        // 시작 시 ID 필드 선택
        idInputTMP.Select();
        idInputTMP.ActivateInputField();
    }
    
    private void Update()
    {
        // Tab 키 처리
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleTabNavigation();
        }
    }
    
    private void HandleTabNavigation()
    {
        if (idInputTMP.isFocused)
        {
            idInputTMP.DeactivateInputField();
            nameInputTMP.Select();
            nameInputTMP.ActivateInputField();
        }
        else if (nameInputTMP.isFocused)
        {
            nameInputTMP.DeactivateInputField();
            pwInputTMP.Select();
            pwInputTMP.ActivateInputField();
        }
        else if (pwInputTMP.isFocused)
        {
            pwInputTMP.DeactivateInputField();
            pwConfirmInputTMP.Select();
            pwConfirmInputTMP.ActivateInputField();
        }
        else if (pwConfirmInputTMP.isFocused)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                pwConfirmInputTMP.DeactivateInputField();
                pwInputTMP.Select();
                pwInputTMP.ActivateInputField();
            }
            else
            {
                pwConfirmInputTMP.DeactivateInputField();
                idInputTMP.Select();
                idInputTMP.ActivateInputField();
            }
        }
    }
    
    private void ValidateName(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            if (nameErrorMessage) nameErrorMessage.SetActive(false);
            return;
        }
        
        bool isValid = Regex.IsMatch(input, namePattern);
        bool hasCorrectLength = input.Length >= 2 && input.Length <= 10;
        
        if (nameErrorMessage)
        {
            nameErrorMessage.SetActive(!isValid || !hasCorrectLength);
        }
        
        nameInputTMP.textComponent.color = (isValid && hasCorrectLength) ? validColor : errorColor;
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
        
        // 색상 설정
        idInputTMP.textComponent.color = (isValid && hasCorrectLength) ? validColor : errorColor;
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
        
        // 색상 설정
        pwInputTMP.textComponent.color = (isValid && hasCorrectLength) ? validColor : errorColor;
        
        // 비밀번호가 변경되면 비밀번호 확인도 다시 검증
        if (!string.IsNullOrEmpty(pwConfirmInputTMP.text))
        {
            ValidatePWConfirm(pwConfirmInputTMP.text);
        }
    }
    
    private void ValidatePWConfirm(string input)
    {
        // 입력이 비어있으면 검증하지 않음
        if (string.IsNullOrEmpty(input))
        {
            if (pwConfirmErrorMessage) pwConfirmErrorMessage.SetActive(false);
            return;
        }
        
        // 비밀번호와 일치하는지 확인
        bool isMatch = input == pwInputTMP.text;
        
        // 에러 메시지 표시/숨김
        if (pwConfirmErrorMessage)
        {
            pwConfirmErrorMessage.SetActive(!isMatch);
        }
        
        // 색상 설정
        pwConfirmInputTMP.textComponent.color = isMatch ? validColor : errorColor;
    }
    
    public void OnSignupButtonClick()
    {
        string name = nameInputTMP.text;
        string id = idInputTMP.text;
        string password = pwInputTMP.text;
        string passwordConfirm = pwConfirmInputTMP.text;
        
        bool isNameValid = Regex.IsMatch(name, namePattern) && name.Length >= 2 && name.Length <= 10;
        bool isIdValid = Regex.IsMatch(id, idPattern) && id.Length >= 6 && id.Length <= 10;
        bool isPwValid = Regex.IsMatch(password, pwPattern) && password.Length >= 8 && password.Length <= 12;
        bool isPwConfirmValid = password == passwordConfirm;
        
        if (isIdValid && isPwValid && isPwConfirmValid)
        {   
            // 회원가입 성공 후 로그인 화면으로 이동하거나 자동 로그인 처리
            using (var user_reader = dbManager.select("user", "count(*)", $"user_id='{id}'"))
            {
                if (user_reader == null || !user_reader.HasRows)  // 데이터가 없는 경우 체크
                {
                    Debug.Log("회원가입 실패: DB와의 통신 실패");
                    // 화면에 오류 메시지 출력
                }
                else
                {
                    while (user_reader.Read())
                    {
                        int count = Convert.ToInt32(user_reader["count(*)"]);
                        if (count == 0)
                        {
                            int error = Convert.ToInt32(dbManager.insert("user", "user_id, username, user_pw", $"'{id}', '{name}', '{password}'"));
                            if (error == 0)
                            {
                                User.Instance.setId(id);
                                User.Instance.setPw(password);
                                User.Instance.setName(name);

                                Debug.Log("회원가입 성공!");
                                SceneManager.LoadScene("UserDetail"); // 로그인 씬으로 이동
                            }
                            else
                            {
                                Debug.Log("회원가입 실패: DB 오류");
                                // 화면에 오류 메시지 출력
                            }
                        }
                        else
                        {
                            Debug.Log("회원가입 실패: 중복된 ID가 있읍니다.");
                            // '중복된 ID가 있습니다' 오류 메시지 출력
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("회원가입 실패: 입력 정보가 올바르지 않습니다.");
            
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
            
            // 비밀번호 확인 에러 메시지 처리
            if (pwConfirmErrorMessage && !string.IsNullOrEmpty(passwordConfirm) && !isPwConfirmValid)
            {
                pwConfirmErrorMessage.SetActive(true);
                StartCoroutine(FlashErrorMessage(pwConfirmErrorMessage));
            }
            
            // 이름 에러 메시지 처리
            if (nameErrorMessage && !string.IsNullOrEmpty(name) && !isNameValid)
            {
                nameErrorMessage.SetActive(true);
                StartCoroutine(FlashErrorMessage(nameErrorMessage));
            }
        }
    }

    // 에러 메시지를 1초 동안 빨간색으로 변경하는 코루틴
    private System.Collections.IEnumerator FlashErrorMessage(GameObject errorMessageObj)
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
        nameInputTMP.onValueChanged.RemoveListener(ValidateName);
        idInputTMP.onValueChanged.RemoveListener(ValidateID);
        pwInputTMP.onValueChanged.RemoveListener(ValidatePW);
        pwConfirmInputTMP.onValueChanged.RemoveListener(ValidatePWConfirm);
    }
}