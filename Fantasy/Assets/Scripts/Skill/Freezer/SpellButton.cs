
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class SpellButton : MonoBehaviour
{

    /// <summary>
    /// ����� ����������� �����������.
    /// </summary>
    public float freezeTimeCounter;

    private float timeElapsed;
    /// <summary>
    /// ���������� ������� ������� �������� �� �����������.
    /// </summary>
    public float TimeElapsed { get { return timeElapsed; } }

    /// <summary>
    /// �������� � ��������� ��������.
    /// </summary>
    public Image FreezePNG;

    /// <summary>
    /// ������� ������� ���������� � ������ ��������� �����.
    /// </summary>
    public UnityEvent OnSpellCasted;

    public void SpellCasted()
    {
        timeElapsed = Mathf.Max(freezeTimeCounter, 0.1f);
        FreezePNG.enabled = true;

        OnSpellCasted.Invoke();
    }

    public void ResetCooldown()
    {
        timeElapsed = 0.0f;
        timeUpdated();
    }

    void Start()
    {
        FreezePNG.enabled = false;
    }

    private void timeUpdated()
    {
        if (timeElapsed <= 0.0f)
        {
            timeElapsed = 0.0f;
            FreezePNG.enabled = false;
        }
        else
        {
            FreezePNG.fillAmount = timeElapsed / freezeTimeCounter;
        }
    }

    void Update()
    {
        if (timeElapsed > 0.0f)
        {
            timeElapsed -= Time.deltaTime;
        }

        timeUpdated();
    }
}
