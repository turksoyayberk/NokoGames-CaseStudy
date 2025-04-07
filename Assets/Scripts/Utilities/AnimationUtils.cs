using System;
using DG.Tweening;
using Game.Common;
using UnityEngine;
using TMPro;

namespace Utilities
{
    public static class AnimationUtils
    {
        private const float JumpPower = 0.5f;
        private const int NumJumps = 1;

        private const float ScaleDuration = 0.2f;
        private static readonly Vector3 ScaleUpSize = new(1.3f, 1.3f, 1.3f);

        public static void MoveItemWithBounce(Action<Item> callBack, Item item, Vector3 targetPosition,
            float duration = 0.5f)
        {
            item.transform.DOLocalJump(
                targetPosition,
                JumpPower,
                NumJumps,
                duration
            ).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                item.SetPlaced(true);
                callBack?.Invoke(item);
            });

            item.transform.DOLocalRotate(
                new Vector3(0, 360, 0),
                duration,
                RotateMode.FastBeyond360
            ).SetEase(Ease.Linear);
        }

        public static void AnimateTextPop(TextMeshProUGUI text)
        {
            text.transform.DOScale(ScaleUpSize, ScaleDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => text.transform.DOScale(Vector3.one, ScaleDuration).SetEase(Ease.InQuad));
        }
    }
}