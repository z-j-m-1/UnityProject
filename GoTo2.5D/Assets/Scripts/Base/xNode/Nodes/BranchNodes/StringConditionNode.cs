using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;


[CreateNodeMenu("分支条件/字符条件")]
public class StringConditionNode : FlowNode
{
    [Output]
    public bool[] conditions;  // 输出端口：每个文本的判断结果

    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public string sentence;    // 输入端口：待判断的句子

    [TextArea(1, 3)]
    public string[] text;      // 预设的文本数组，在Inspector中配置

    // 获取输出端口的值
    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(conditions))
        {
            // 1. 获取输入的句子
            string inputSentence = GetInputValue<string>("sentence", sentence);

            // 2. 如果句子为空或text数组为空，返回空数组
            if (string.IsNullOrEmpty(inputSentence) || text == null || text.Length == 0)
                return new bool[0];

            // 3. 对每个预设文本进行判断（这里使用是否包含，你可以改成完全匹配）
            bool[] results = new bool[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                 results[i] = inputSentence == text[i];
            }

            return results;
        }
        return null;
    }
}