
using System.Collections.Generic;
// נותן גישה ל-Dictionary שסופר כמה קוליידרים של כל שחקן נמצאים באזור.
using UnityEngine;
// נותן גישה ל-MonoBehaviour, ל-Collider ול-BoxCollider של Unity.

[DisallowMultipleComponent]
// מונע שני רכיבי HideZone על אותו אזור מסתור.
[RequireComponent(typeof(BoxCollider))]
// מבטיח שלאזור יהיה BoxCollider שמזהה כניסה ויציאה.
public sealed class HideZone : MonoBehaviour
// מגדיר אזור שבו השחקן יכול להיות מוסתר רק כאשר הוא גם כורע.
{
    // פתיחת גוף המחלקה HideZone.
    private readonly Dictionary<PlayerStealthState, int> playerContacts = new Dictionary<PlayerStealthState, int>();
    // שומר לכל שחקן כמה מהקוליידרים שלו עדיין נמצאים בתוך האזור.

    private void Reset()
    // פועל כאשר מוסיפים את הסקריפט או לוחצים Reset ב-Inspector.
    {
        // פתיחת המתודה Reset.
        GetComponent<BoxCollider>().isTrigger = true;
        // הופך את הקוליידר לחיישן שאינו חוסם את השחקן.
    }
    // סיום המתודה Reset.

    private void Awake()
    // פועל פעם אחת כאשר אזור המחבוא נטען.
    {
        // פתיחת המתודה Awake.
        GetComponent<BoxCollider>().isTrigger = true;
        // מוודא שהקוליידר נשאר Trigger גם בזמן המשחק.
    }
    // סיום המתודה Awake.

    private void OnTriggerEnter(Collider other)
    // פועל כאשר קוליידר כלשהו נכנס לאזור המחבוא.
    {
        // פתיחת המתודה OnTriggerEnter.
        PlayerStealthState stealth = other.GetComponentInParent<PlayerStealthState>();
        // מחפש את מצב ההתגנבות על הגוף שנכנס או על אובייקט האב שלו.

        if (stealth == null)
        // בודק אם הגוף שנכנס אינו שייך לשחקן.
        {
            // פתיחת תנאי גוף שאינו שחקן.
            return;
            // מתעלם מ-Lauren ומחפצים אחרים.
        }
        // סיום תנאי גוף שאינו שחקן.

        playerContacts.TryGetValue(stealth, out int contactCount);
        // קורא כמה קוליידרים של אותו שחקן כבר נמצאים באזור או מקבל אפס.
        playerContacts[stealth] = contactCount + 1;
        // מוסיף את הקוליידר החדש למונה של אותו שחקן.

        if (contactCount == 0)
        // בודק אם זו הכניסה הראשונה של השחקן לאזור הזה.
        {
            // פתיחת תנאי הכניסה הראשונה.
            stealth.SetHidden(true);
            // מודיע למצב ההתגנבות שהשחקן נמצא כעת בתוך אזור מחבוא אחד נוסף.
        }
        // סיום תנאי הכניסה הראשונה.
    }
    // סיום המתודה OnTriggerEnter.

    private void OnTriggerExit(Collider other)
    // פועל כאשר קוליידר כלשהו יוצא מאזור המחבוא.
    {
        // פתיחת המתודה OnTriggerExit.
        PlayerStealthState stealth = other.GetComponentInParent<PlayerStealthState>();
        // מחפש את מצב ההתגנבות של הגוף שיצא.

        if (stealth == null || !playerContacts.TryGetValue(stealth, out int contactCount))
        // בודק אם זה אינו שחקן רשום באזור.
        {
            // פתיחת תנאי היציאה שאינה רלוונטית.
            return;
            // יוצא בלי לשנות את מצב ההסתרה.
        }
        // סיום תנאי היציאה שאינה רלוונטית.

        contactCount--;
        // מפחית קוליידר אחד ממספר המגעים של השחקן באזור.

        if (contactCount > 0)
        // בודק אם קוליידר אחר של אותו שחקן עדיין נמצא באזור.
        {
            // פתיחת תנאי המגעים שנותרו.
            playerContacts[stealth] = contactCount;
            // שומר את המונה החדש ומשאיר את השחקן בתוך האזור.
            return;
            // עוצר כי השחקן עדיין לא יצא לגמרי.
        }
        // סיום תנאי המגעים שנותרו.

        playerContacts.Remove(stealth);
        // מסיר את השחקן מהמילון לאחר שהקוליידר האחרון יצא.
        stealth.SetHidden(false);
        // מודיע למצב ההתגנבות שהשחקן יצא מאזור המחבוא הזה.
    }
    // סיום המתודה OnTriggerExit.

    private void OnDisable()
    // פועל אם אזור המחבוא נכבה בזמן ששחקן נמצא בתוכו.
    {
        // פתיחת המתודה OnDisable.
        foreach (PlayerStealthState stealth in playerContacts.Keys)
        // עובר על כל השחקנים שעדיין רשומים באזור.
        {
            // פתיחת לולאת הניקוי.
            if (stealth != null)
            // בודק שהשחקן עדיין קיים.
            {
                // פתיחת תנאי השחקן התקין.
                stealth.SetHidden(false);
                // מבטל את המגע של האזור הזה כדי שלא תישאר הסתרה מזויפת.
            }
            // סיום תנאי השחקן התקין.
        }
        // סיום לולאת הניקוי.

        playerContacts.Clear();
        // מנקה את כל המגעים הישנים לפני הפעלה מחדש.
    }
    // סיום המתודה OnDisable.
}
// סיום גוף המחלקה HideZone.
