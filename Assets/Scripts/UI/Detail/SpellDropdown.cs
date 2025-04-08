using UnityEngine;
using TMPro;   // TMP 컴포넌트를 사용하기 위해 추가
using System.Collections.Generic;  // List 사용을 위해 추가

public class SpellDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;  // TMP 드롭다운 컴포넌트
    
    // 선택된 철자를 알려주는 이벤트 추가
    public delegate void SpellSelectedHandler(string spell);
    public event SpellSelectedHandler OnSpellSelected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // TMP 드롭다운 컴포넌트 가져오기
        dropdown = GetComponent<TMP_Dropdown>();
        
        // 드롭다운 옵션 초기화
        List<string> options = new List<string>();
        
        // 한글 자음 추가 (ㄱ-ㅎ)
        string[] koreanConsonants = { "ㄱ", "ㄴ", "ㄷ", "ㄹ", "ㅁ", "ㅂ", "ㅅ", "ㅇ", "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
        options.AddRange(koreanConsonants);
        
        // 영문 알파벳 추가 (A-Z)
        for (char c = 'A'; c <= 'Z'; c++)
        {
            options.Add(c.ToString());
        }
        
        // 드롭다운에 옵션 설정
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        
        dropdown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(OnSpellDropdownChanged);
    }

    private void OnSpellDropdownChanged(int index)
    {
        string selectedSpell = dropdown.GetComponent<TMP_Dropdown>().options[index].text;
        OnSpellSelected?.Invoke(selectedSpell);
    }
}
