using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("SLIDER")]
    public Slider slider;          // Slider referansi
    public TMP_Text topTMP;        // Ust kisimdaki Text referansi
    public TMP_Text bottomTMP;     // Alt kisimdaki Text referansi
    private int counter = 1;       // Kac kere doldugunu sayacak degisken

    public void UpdateSlider(float newValue)
    {
        // Yeni degeri eklemeden once toplam deger hesaplaniyor
        float totalValue = slider.value + newValue;

        // Kac kez tam doldugunu hesapla
        int fullCounts = Mathf.FloorToInt(totalValue / slider.maxValue);

        // Sayac tam dolum sayisina gore artiriliyor
        counter += fullCounts;

        // Kalan degeri hesapla
        float overflowValue = totalValue % slider.maxValue;

        // Slider'i kalan degere ayarla
        slider.value = overflowValue;

        // UI'a counter ve next value guncelleniyor
        bottomTMP.text = counter.ToString();
        topTMP.text = (counter + 1).ToString();
    }
}
