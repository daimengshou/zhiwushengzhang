using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuideScene
{
    public class NextButton : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> displayList;

        public static int displayIndex = 0;

        public int SceneIndex;

        public void ClickNext()
        {
            if (displayList == null || displayList.Count == 0)
            {
                SceneSwitch.SwitchTo(SceneIndex);
                return;
            }

            if (displayIndex == displayList.Count - 1)
            {
                SceneSwitch.SwitchTo(SceneIndex);
            }
            else
            {
                PositionAdjust();
                StartCoroutine(GameObjectSwitchNext());
            }
        }

        public void ClickPrevious()
        {
            if (displayList == null || displayList.Count == 0) return;

            if (displayIndex != 0)
            {
                StartCoroutine(GameObjectSwitchPrevious());
            }
        }

        private void PositionAdjust()
        {
            /*
             * 根据待消失的RectTransform
             * 计算出其右下角的坐标
             */
            RectTransform rtDisappear = displayList[displayIndex].GetComponent<RectTransform>();

            Vector2 pivotDisappear = rtDisappear.pivot;
            Rect rectDisappear = rtDisappear.rect;

            Vector2 rightBottom = rtDisappear.anchoredPosition +
                new Vector2((1 - pivotDisappear.x) * rectDisappear.width, -(1 - pivotDisappear.y) * rectDisappear.height);

            /*
             * 根据右下角坐标（待出现的左下角坐标）
             * 反算出位置
             */
            RectTransform rtAppear = displayList[displayIndex + 1].GetComponent<RectTransform>();

            Vector2 pivotAppear = rtAppear.pivot;
            Rect rectAppear = rtAppear.rect;

            Vector2 anchoredPos = rightBottom + Vector2.Scale(new Vector2(rectAppear.width, rectAppear.height), pivotAppear);
            rtAppear.anchoredPosition = anchoredPos;
            displayList[displayIndex + 1].SetActive(true);
        }

        const int AnimatorCount = 10;

        private IEnumerator GameObjectSwitchNext()
        {
            RectTransform rtDisappear = displayList[displayIndex].GetComponent<RectTransform>();
            RectTransform rtAppear = displayList[displayIndex + 1].GetComponent<RectTransform>();

            Vector2 posDetla = new Vector2(-(rtDisappear.rect.width + rtAppear.rect.width) / (2.0f * AnimatorCount), 0);

            for (int i = 1; i <= AnimatorCount; i++)
            {
                rtDisappear.anchoredPosition += posDetla;
                rtAppear.anchoredPosition += posDetla;

                yield return null;
            }

            displayList[displayIndex].SetActive(false);
            displayIndex++;
        }

        IEnumerator GameObjectSwitchPrevious()
        {
            displayList[displayIndex - 1].SetActive(true);

            RectTransform rtDisappear = displayList[displayIndex].GetComponent<RectTransform>();
            RectTransform rtAppear = displayList[displayIndex - 1].GetComponent<RectTransform>();

            Vector2 posDetla = new Vector2((rtDisappear.rect.width + rtAppear.rect.width) / (2.0f * AnimatorCount), 0);

            for (int i = 1; i <= AnimatorCount; i++)
            {
                rtDisappear.anchoredPosition += posDetla;
                rtAppear.anchoredPosition += posDetla;

                yield return null;
            }

            displayList[displayIndex].SetActive(false);
            displayIndex--;
        }
    }
}


