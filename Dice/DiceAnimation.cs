using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cardinals.Enums;
using DG.Tweening;

namespace Cardinals
{
    public class DiceAnimation : MonoBehaviour
    {
        public IEnumerator Play(DiceAnimationType animationType)
        {
            switch (animationType)
            {
                case DiceAnimationType.UseAttack:
                    yield return CardUseToAttackAnimation();
                    yield break;

                case DiceAnimationType.UseDefense:
                    yield return CardUseToDefenseAnimation();
                    yield break;

                case DiceAnimationType.UseMove:
                    yield return CardUseToMoveAnimation();
                    yield break;

                case DiceAnimationType.TurnEnd:
                    EndTurnAnimation();
                    yield break;
            }
        }

        private IEnumerator CardUseToAttackAnimation()
        {
            /*Vector3 center = Vector3.zero;
            GameManager.I.CurrentEnemies.ToList().ForEach((x) =>
            {
                center += x.transform.position;
            });*/
            /*Vector3 enemyPos = Camera.main.WorldToScreenPoint(GameManager.I.Stage.Enemies[GameManager.I.Stage.Board.BoardInputHandler.HoveredIdx].transform.position);
            enemyPos.y -= 50f;
            (transform as RectTransform).DOMove(enemyPos, 0.15f);*/
            transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.15f);
            yield return new WaitForSeconds(0.3f);

            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator CardUseToDefenseAnimation()
        {
            Vector3 enemyPos = Camera.main.WorldToScreenPoint(GameManager.I.Stage.Enemies[GameManager.I.Stage.Board.BoardInputHandler.HoveredIdx].transform.position);
            enemyPos.y -= 50f;
            (transform as RectTransform).DOMove(enemyPos, 0.15f);
            transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.15f);
            yield return new WaitForSeconds(0.3f);

            (transform as RectTransform).DOJump(Camera.main.WorldToScreenPoint(GameManager.I.Player.transform.position), 100f, 1, 0.3f);
            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(0.3f);
        }

        private IEnumerator CardUseToMoveAnimation()
        {
            transform.DOScale(Vector3.zero, 0.3f);
            yield return new WaitForSeconds(0.4f);
        }

        private void EndTurnAnimation()
        {
            transform.DOMoveY(-120f, 0.3f).SetEase(Ease.OutBack);
            //transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutCubic);

        }
    }

}
