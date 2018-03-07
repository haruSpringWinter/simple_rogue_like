using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    // 動作にかける時間を保持
    public float moveTime = 0.1f;

    // レイヤーマスクを定義
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;

    // moveTimeの逆数
    private float inverseMoveTime;


	// Use this for initialization
	protected virtual void Start () {

        // コンポーネントを取得
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
	}

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        // rayで当たり判定を操作する際に、自分自身に当たらないよう一旦Colidderをオフに
        boxCollider.enabled = false;
        // レイヤー上に始点と終点を設定し、コライダーがヒットした場合にtrue、なければnull
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if(hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        return false;
    }

    // endまで移動
    protected IEnumerator SmoothMovement (Vector3 end)
    {
        // sqrMagnitudeは a^2 + b^2の値を返す ; 計算量が低くすむ
        // magnitudeはルートをとった値を返す
        float sqrRemainingDist = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDist > float.Epsilon)
        {
            // inverseMoveTimeは距離、deltaTimeは一歩にかける時間を表す
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDist = (transform.position - end).sqrMagnitude;

            // 1frame待機するおまじない
            yield return null;
        }
    }

    // 接触先のコンポーネントを後々定義するため、タイプパラメータTを持たす
    protected virtual void AttemptMove<T> (int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;

        // 接触したオブジェクトのコンポーネントをゲット
        T hitComponent = hit.transform.GetComponent<T>();

        // 何かにあたって移動ができない場合
        if(!canMove && hitComponent != null)
        {
            OnCantMove(hitComponent);
        }
    }

    // 障害物や敵に当たった時の行動
    protected abstract void OnCantMove<T>(T Component)
        where T : Component;
}
