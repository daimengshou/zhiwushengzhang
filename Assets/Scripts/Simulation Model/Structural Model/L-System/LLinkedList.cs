/*
 * 文件名：LLinkedList.cs
 * 描述：L-系统底层数据结构
 */
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * L-系统专用双向链表
 * 用于存放与处理由多个模块（LTerm）组成的数据
 * 
 * @version: 1.0
 */
public class LLinkedList<T> where T : class
{
    private LLinkedListNode<T> m_head;  //头节点
    private int m_count;                //节点个数

    /// <summary>
    /// 空构造函数，唯一
    /// </summary>
    public LLinkedList()
    {
    }

    /// <summary>
    /// 节点个数
    /// </summary>
    public int Count
    {
        get { return m_count; }
    }

    /// <summary>
    /// 头节点
    /// </summary>
    public LLinkedListNode<T> First
    {
        get { return m_head; }
    }

    /// <summary>
    /// 尾节点
    /// </summary>
    public LLinkedListNode<T> Last
    {
        get { return m_head.Previous; }
    }

    /// <summary>
    /// 添加一个节点
    /// </summary>
    /// <param name="value">待添加节点的值</param>
    /// <returns>添加后的节点</returns>
    public LLinkedListNode<T> Add(T value)
    {
        if (m_head == null)
            return AddAfter(null, value);
        else
            return AddAfter(m_head.Previous, value);
    }

    /// <summary>
    /// 添加一个节点
    /// </summary>
    /// <param name="item">待添加的节点</param>
    public void Add(LLinkedListNode<T> item)
    {
        if (m_head == null)
            AddAfter(null, item);
        else
            AddAfter(m_head.Previous, item);
    }

    /// <summary>
    /// 添加一个链表
    /// </summary>
    /// <param name="list">待添加的链表</param>
    public void Add(LLinkedList<T> list)
    {
        if (m_head == null)
            AddAfter(null, list);
        else
            AddAfter(m_head.Previous, list);
    }

    /// <summary>
    /// 在一个节点之后添加一个节点
    /// </summary>
    /// <param name="node">在该节点后添加</param>
    /// <param name="value">待添加节点的值</param>
    /// <returns>添加后的节点</returns>
    public LLinkedListNode<T> AddAfter(LLinkedListNode<T> node, T value)
    {
        LLinkedListNode<T> newNode = new LLinkedListNode<T>(value);

        AddAfter(node, newNode);

        return newNode;
    }

    /// <summary>
    /// 在一个节点之后添加一个节点
    /// </summary>
    /// <param name="node">在该节点后添加</param>
    /// <param name="newNode">待添加的节点</param>
    public void AddAfter(LLinkedListNode<T> node, LLinkedListNode<T> newNode)
    {
        ValidateNode(node); //对列表中的节点进行检查

        if (m_count == 0)   //列表为空
            InsertNodeToEmptyList(newNode);
        else                //列表不为空
            InsertNodeToListAfter(node, newNode);
    }

    /// <summary>
    /// 在一个节点之后添加一个链表
    /// </summary>
    /// <param name="node">在该节点之后添加</param>
    /// <param name="list">待添加的链表</param>
    public void AddAfter(LLinkedListNode<T> node, LLinkedList<T> list)
    {
        ValidateNode(node); //对列表中的节点进行检查

        LLinkedList<T> tempList = list.Clone();

        if (m_count == 0)
            InsertListToEmptyList(tempList);
        else
            InsertListToListAfter(node, tempList);
    }

    /// <summary>
    /// 将一个节点置换成另外一个节点
    /// </summary>
    /// <param name="node">被置换的节点</param>
    /// <param name="value">置换节点的值</param>
    public void Replace(LLinkedListNode<T> node, T value)
    {
        Replace(node, new LLinkedListNode<T>(value));
    }

    /// <summary>
    /// 将一个节点置换成另外一个节点
    /// </summary>
    /// <param name="node">被置换的节点</param>
    /// <param name="newNode">置换的节点</param>
    public void Replace(LLinkedListNode<T> node, LLinkedListNode<T> newNode)
    {
        ValidateNode(node); //对列表中的节点进行检查

        if (m_count == 1)
            ReplaceNodeWithNodeInSingleNodeList(node, newNode);
        else
            ReplaceNodeWithNodeInMultiNodesList(node, newNode);
    }

    /// <summary>
    /// 将一个节点置换成另外一个链表
    /// </summary>
    /// <param name="node">被置换的节点</param>
    /// <param name="list">置换的链表</param>
    public void Replace(LLinkedListNode<T> node, LLinkedList<T> list)
    {
        ValidateNode(node);

        if (m_count == 1)
            ReplaceNodeWithListInSingleNodeList(node, list);
        else
            ReplaceNodeWithListInMultiNodesList(node, list);
    }

    /// <summary>
    /// 将当前链表克隆
    /// </summary>
    /// <returns></returns>
    public LLinkedList<T> Clone()
    {
        LLinkedList<T> resultList = new LLinkedList<T>();

        LLinkedListNode<T> srcNode = m_head;    //源节点从该链表的头结点开始
        do 
        {
            LLinkedListNode<T> destNode = new LLinkedListNode<T>(srcNode.Value);    //复制源节点的值，不复制其前驱和后继
            resultList.Add(destNode);   //插入复制后的节点

            srcNode = srcNode.Next;     //移动到下一个节点
        } while (srcNode != m_head);    //当源节点重新回归到头结点时，停止复制

        return resultList;
    }

    /// <summary>
    /// 往空链表（当前链表）插入一个节点
    /// </summary>
    /// <param name="node">待插入的节点</param>
    private void InsertNodeToEmptyList(LLinkedListNode<T> node)
    {
        node.Previous = node;
        node.Next = node;

        m_head = node;
        m_count++;
    }

    /// <summary>
    /// 往空链表（当前链表）插入一个链表
    /// </summary>
    /// <param name="list">待插入的链表</param>
    private void InsertListToEmptyList(LLinkedList<T> list)
    {
        m_head = list.First;
        m_count = list.Count;
    }

    /// <summary>
    /// 往一个节点之后插入一个节点
    /// </summary>
    /// <param name="node">在该节点之后插入</param>
    /// <param name="newNode">待插入的节点</param>
    private void InsertNodeToListAfter(LLinkedListNode<T> node, LLinkedListNode<T> newNode)
    {
        newNode.Previous = node;
        newNode.Next = node.Next;

        node.Next.Previous = newNode;
        node.Next = newNode;
        m_count++;
    }

    /// <summary>
    /// 往一个节点之后插入一个链表
    /// </summary>
    /// <param name="node">在该节点之后插入</param>
    /// <param name="list">待插入的节点</param>
    private void InsertListToListAfter(LLinkedListNode<T> node, LLinkedList<T> list)
    {
        LLinkedListNode<T> listHead = list.First;
        LLinkedListNode<T> listEnd = list.Last;

        //链表头尾变化
        listHead.Previous = node;
        listEnd.Next = node.Next;

        node.Next.Previous = listEnd;
        node.Next = listHead;

        m_count += list.Count;
    }

    /// <summary>
    /// 在单一节点的链表中将一个节点置换成另外一个节点
    /// </summary>
    /// <param name="node">被置换的节点</param>
    /// <param name="newNode">置换的节点</param>
    private void ReplaceNodeWithNodeInSingleNodeList(LLinkedListNode<T> node, LLinkedListNode<T> newNode)
    {
        newNode.Previous = newNode; //前驱
        newNode.Next     = newNode; //后继
        m_head = newNode;

        //node.Invalidate();  //初始化被置换的节点
    }

    /// <summary>
    /// 在单一节点的链表中将一个节点置换成另外一个链表
    /// </summary>
    /// <param name="node">被置换的节点</param>
    /// <param name="list">置换的链表</param>
    private void ReplaceNodeWithListInSingleNodeList(LLinkedListNode<T> node, LLinkedList<T> list)
    {
        m_head = list.First;    //当前链表的头节点与置换链表的头节点相同

        m_count = list.Count;   //当前链表的节点数与置换链表的节点数相同
    }

    /// <summary>
    /// 在多节点的链表中将一个节点置换成另外一个节点
    /// </summary>
    /// <param name="node">被置换的节点</param>
    /// <param name="newNode">置换的节点</param>
    private void ReplaceNodeWithNodeInMultiNodesList(LLinkedListNode<T> node, LLinkedListNode<T> newNode)
    {
        newNode.Previous = node.Previous;   //前驱
        newNode.Next     = node.Next;       //后继

        node.Previous.Next = newNode;
        node.Next.Previous = newNode;

        if (m_head == node)
            m_head = newNode;
    }

    /// <summary>
    /// 在多节点的链表中将一个节点置换成另外一个链表
    /// </summary>
    /// <param name="node">被置换的节点</param>
    /// <param name="list">置换的链表</param>
    private void ReplaceNodeWithListInMultiNodesList(LLinkedListNode<T> node, LLinkedList<T> list)
    {
        LLinkedListNode<T> listHead = list.First;   //首节点
        LLinkedListNode<T> listEnd  = list.Last;    //尾节点

        listHead.Previous = node.Previous;
        listEnd.Next      = node.Next;

        node.Previous.Next = listHead;
        node.Next.Previous = listEnd;

        if (m_head == node)
            m_head = listHead;

        m_count += (list.Count - 1);
    }

    /// <summary>
    /// 检验节点是否可用
    /// </summary>
    /// <param name="node">待检验的节点</param>
    private void ValidateNode(LLinkedListNode<T> node)
    {
        if (node == null && m_count != 0) throw new InvalidOperationException("The input node is NULL.");

        if (node != null && m_count == 0) throw new InvalidOperationException("The list is empty.");
    }

    /// <summary>
    /// 清除
    /// </summary>
    public void Clear()
    {
        LLinkedListNode<T> node = m_head;

        while(node != null)
        {
            LLinkedListNode<T> tempNode = node;
            node = node.Next;

            tempNode.Invalidate();
        }
    }

    public List<T> ToList()
    {
        List<T> result = new List<T>();

        LLinkedListNode<T> node = m_head;
        do 
        {
            result.Add(node.Value);

            node = node.Next;
        } while (node != m_head);

        return result;
    }

    public T[] ToArray()
    {
        T[] result = new T[Count];

        LLinkedListNode<T> node = m_head;
        for (int i = 0; i < Count; i++)
        {
            result[i] = node.Value;
            node = node.Next;
        }

        return result;
    }

    public override string ToString()
    {
        if (m_count == 0)  return "";   //列表为空

        string result = "";
        LLinkedListNode<T> node = m_head;
        do 
        {
            if (node.Value != null)
                result += node.Value.ToString();

            node = node.Next;
        } while (node != m_head);

        return result;
    } 
}

/*
 * L-系统双向链表专用节点类
 * 
 * @version: 1.0
 */
public class LLinkedListNode<T> where T : class
{
    private LLinkedListNode<T> m_prev;    //前驱
    private LLinkedListNode<T> m_next;    //后继
    private T m_value;

    public LLinkedListNode(T value)
    {
        m_value = value;
    }

    public LLinkedListNode<T> Previous
    {
        get { return m_prev; }
        set { m_prev = value; }
    }

    public LLinkedListNode<T> Next
    {
        get { return m_next; }
        set { m_next = value; }
    }

    public T Value
    {
        get { return m_value; }
        set { m_value = value; }
    }

    public void Invalidate()
    {
        m_prev = null;
        m_next = null;
        m_value = null;
    }
}