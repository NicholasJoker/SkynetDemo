using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
namespace Script.Game
{
    public class Card : MonoBehaviour
    {
        public Image image;        //牌的图片
        public CardInfo cardInfo;  //卡牌信息
        private float offsetY;
        private Button cardButton;
        void Awake()
        {
            // image = GetComponent<Image>();
            // //计算世界坐标系移动的位置
            // var distance = GetComponent<RectTransform>().sizeDelta.y / 10;
            // var pos1 = transform.position;
            // var pos2 = transform.position + Vector3.up * distance;
            // var pos11 = transform.TransformPoint(pos1);
            // var pos22 = transform.TransformPoint(pos2);
            // offsetY = pos22.y - pos11.y;
        }

        void Start()
        {
            //计算世界坐标系移动的位置
            var distance = GetComponent<RectTransform>().sizeDelta.y / 10;
            var pos1 = transform.position;
            var pos2 = transform.position + Vector3.up * distance;
            var pos11 = transform.TransformPoint(pos1);
            var pos22 = transform.TransformPoint(pos2);
            offsetY = pos22.y - pos11.y;
            cardButton = GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(OnButtonClick);
            }
        }
        /// <summary>
        /// 初始化图片
        /// </summary>
        /// <param name="cardInfo"></param>
        public void InitImage(CardInfo cardInfo)
        {
            // Debug.Log($"cardInfo:{cardInfo.cardName}");
            this.cardInfo = cardInfo;
            // Debug.Log(this.cardInfo.cardName);
            var sprite = Resources.Load("Card/" + this.cardInfo.cardName,typeof(Sprite)) as Sprite;
            image.sprite = sprite; //Resources.Load("Card/" + this.cardInfo.cardName,typeof(Sprite)) as Sprite;
            //image.overrideSprite = Resources.Load("Card/" + cardInfo.cardName, typeof(Sprite)) as Sprite;
        }
        /// <summary>
        /// 设置选择状态
        /// </summary>
        public void SetSelectState()
        {
            if (!DOTween.IsTweening(transform))
            {
                if (cardInfo.isSelected)
                {
                    transform.DOMoveY(transform.position.y - offsetY, 0.2f);
                }
                else
                {
                    transform.DOMoveY(transform.position.y + offsetY, 0.2f);
                }
                cardInfo.isSelected = !cardInfo.isSelected;
            }
        }
        /// <summary>
        /// 点击
        /// </summary>
        public void OnButtonClick()
        {
            SetSelectState();
        }
    }
}