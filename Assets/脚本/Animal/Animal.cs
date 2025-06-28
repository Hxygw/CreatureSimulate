using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// ��������
/// </summary>
public class AnimalType
{
    public const float StrepsipteraLoveCoast = 0.2f;
    /// <summary>
    /// ����ͻ��ĸ���(0.00--1.00)
    /// </summary>
    private const double Probability = 0.1d;
    const string BodyPartAddress = "D:\\Unity�ļ���\\CreatureSimulate\\Assets\\AnimalDate\\BodyParts.txt";
    const string AnimalTypeAddress = "D:\\Unity�ļ���\\CreatureSimulate\\Assets\\AnimalDate\\AnimalTypes.txt";
    static string BodyPartText;
    static string AnimalTypeText;
    public static Dictionary<string, BodyPart> BodyPartTable = new();
    /// <summary>
    /// ���е����岿��
    /// </summary>
    public static List<BodyPart> BodyParts = new();
    /// <summary>
    /// �Ѿ�������������������
    /// </summary>
    public static List<AnimalType> AnimalTypes = new();



    public readonly Color color;
    public readonly int id;
    public readonly List<BodyPart> bodyParts = new();
    public readonly float speed, attack, range, power, breath, hungrySpeed, acceleration, size, loveSatietyCoast = 0.3f;
    public readonly int blood, weight;
    public bool strepsiptera = false;
    public FoodHabit foodHabit;
    /// <summary>
    /// ���岿��id�б�������������Ĳ���
    /// </summary>
    protected readonly List<int> bodyPartIdList = new();

    public AnimalType(FoodHabit foodHabit, List<BodyPart> bodyParts, int id)
    {
        this.foodHabit = foodHabit;
        blood = 0;
        range = 0;
        weight = 0;
        attack = 0;
        power = 0;
        this.id = id;
        foreach(BodyPart part in bodyParts)
        {
            this.bodyParts.Add(part);
            switch (part.type)
            {
                case "Heart":
                    blood += (int)part.num;
                    break;
                case "Eye":
                    range += part.num;
                    break;
                case "Leg":
                    power += part.num;
                    break;
                case "Horn":
                    attack += part.num;
                    break;
                case "Claw":
                    attack += part.num;
                    break;
                case "Lung":
                    breath += part.num;
                    break;
                case "Strepsiptera"://���Ŀ������:��ֳ������������,���״���������
                    loveSatietyCoast = StrepsipteraLoveCoast;
                    strepsiptera = true;
                    break;
                default:
                    throw new System.Exception("����Ĳ�������");
            }
            blood -= part.blood;
            weight += part.weight;
            bodyPartIdList.Add(part.id);
        }
        if (blood < 0)
            throw new System.ArgumentException("ѪҺ������֧��");
        if (breath == 0)
            throw new System.ArgumentException("û�з�");
        speed = Mathf.Max(power, 1) / weight;
        hungrySpeed = Mathf.Pow(weight, 1.5f) * (Mathf.Log(power + 2, 2) + 10);
        acceleration = Mathf.Max(power, 1) * Mathf.Pow(blood + 1f, 0.8f) / Mathf.Pow(weight, 0.8f);
        size = Mathf.Pow(weight, 0.8f);
        if (range == 0) range = 1;
        bodyPartIdList.Sort();

        color = Random.ColorHSV();
    }

    /// <summary>
    /// ��������ܷ���
    /// </summary>
    /// <param name="bodyParts"></param>
    /// <returns></returns>
    private static bool CheckOut(List<BodyPart> bodyParts)
    {
        if (bodyParts == null || bodyParts.Count < 2) return false;
        int blood = 0;
        bool lung = false;
        foreach (BodyPart part in bodyParts)
            if (part.type == "Heart")
                blood += (int)part.num;
            else
            {
                if (part.type == "Lung")
                    lung = true;
                blood -= part.blood;
            }
        return blood >= 0 && lung;
    }
    /// <summary>
    /// ��ʼ��AnimalType
    /// </summary>
    /// <returns></returns>
    public static bool Setup()
    {
        try { BodyPartText = File.ReadAllText(BodyPartAddress); }
        catch{ throw new System.Exception("BodyParts.txt·������(����������)"); }
        try { AnimalTypeText = File.ReadAllText(AnimalTypeAddress); }
        catch { throw new System.Exception("AnimalTypes.txt·������(����������)"); }
        string[] bodyParts = BodyPartText.Split("\r\n");
        string[] animalTypes = AnimalTypeText.Split("\r\n");
        string[][] words = new string[bodyParts.Length][];
        for (int i = 0; i < words.Length; i++)
            words[i] = bodyParts[i].Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length != 5)
            {
                Debug.Log("BodyParts.txt��" + (i + 1).ToString() + "�д��ڴ�������");
                continue;
            }
            if (BodyPartTable.ContainsKey(words[i][1]))
            {
                Debug.Log("BodyParts.txt��\" + (i + 1).ToString() + \"�������ظ�");
                continue;
            }
            BodyParts.Add(new BodyPart(BodyParts.Count, words[i][0], words[i][1], int.Parse(words[i][2]), int.Parse(words[i][3]), float.Parse(words[i][4])));
            BodyPartTable.Add(words[i][1], BodyParts[^1]);
        }
        words = new string[animalTypes.Length][];
        for (int i = 0; i < animalTypes.Length; i++)
            words[i] = animalTypes[i].Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            List<BodyPart> parts = new();
            for(int j = 2; j < words[i].Length;j++)
                if (BodyPartTable.ContainsKey(words[i][j]))
                    parts.Add(BodyPartTable[words[i][j]]);
                else
                {
                    Debug.Log("AnimalTypes.txt��" + (i + 1).ToString() + "�д��ڴ�������");
                    continue;
                }
            if (CheckOut(parts) && int.TryParse(words[i][1],out int k))
            {
                if (k < 0 || k >= 3)
                {
                    Debug.Log("AnimalTypes.txt��" + (i + 1).ToString() + "�д��ڴ�������");
                    continue;
                }
                AnimalType type = new((FoodHabit)k, parts, AnimalTypes.Count);
                foreach(var t in AnimalTypes)
                    if(ListEquals(t.bodyPartIdList,type.bodyPartIdList))
                    {
                        Debug.Log("AnimalTypes.txt��" + (i + 1).ToString() + "�д����ظ�����");
                        continue;
                    }
                AnimalTypes.Add(type);
            }
            else
                Debug.Log("AnimalTypes.txt��" + (i + 1).ToString() + "�д����޷�ʵ�ֵ���������");
        }
        return true;
    }

    public static AnimalType CreatNewAnimal(AnimalType animalType1,AnimalType animalType2)
    {
        if (animalType1.id == animalType2.id)
            if (Random.value <= Probability)
            {
                BodyPart bodyPart = animalType1.bodyParts[Random.Range(0, animalType1.bodyParts.Count)];
                List<BodyPart> bodyParts = new();
                foreach (BodyPart part in animalType1.bodyParts)
                    bodyParts.Add(part);
                if (Random.value >= 0.5) bodyParts.Add(bodyPart);
                else bodyParts.Remove(bodyPart);
                if (CheckOut(bodyParts))
                {
                    var Type = new AnimalType(animalType1.foodHabit, bodyParts, AnimalTypes.Count);
                    foreach (var type in AnimalTypes)
                        if (ListEquals(Type.bodyPartIdList, type.bodyPartIdList))
                            return type;
                    AnimalTypes.Add(Type);
                    return AnimalTypes[^1];
                }
                else return animalType1;
            }
            else return animalType1;
        else
        {
            Dictionary<System.Type, List<BodyPart>> bodyParts = new();
            List<BodyPart> bodyParts2 = new();
            foreach (BodyPart part in animalType1.bodyParts.Count > animalType2.bodyParts.Count ? animalType2.bodyParts : animalType1.bodyParts)
                if (bodyParts.ContainsKey(part.GetType()))
                    bodyParts[part.GetType()].Add(part);
                else
                    bodyParts.Add(part.GetType(), new() { part });
            foreach (BodyPart part in animalType1.bodyParts.Count <= animalType2.bodyParts.Count ? animalType2.bodyParts : animalType1.bodyParts)
                if (bodyParts.ContainsKey(part.GetType()))
                    bodyParts2.Add(Random.value >= 0.5f ? bodyParts[part.GetType()][Random.Range(0, bodyParts[part.GetType()].Count)] : part);
                else
                    bodyParts2.Add(part);
            if (CheckOut(bodyParts2))
            {
                var Type = new AnimalType(animalType1.foodHabit, bodyParts2, AnimalTypes.Count);
                foreach (var type in AnimalTypes)
                    if (ListEquals(Type.bodyPartIdList, type.bodyPartIdList))
                        return type;
                AnimalTypes.Add(Type);
                return AnimalTypes[^1];
            }
            else return null;
        }
    }



    public static bool ListEquals(List<int> a, List<int> b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (a[i] != b[i])
                return false;
        return true;
    }
}

/// <summary>
/// ���岿��
/// </summary>
public class BodyPart
{
    public int id;
    public string type;
    public string name;
    public int blood, weight;
    /// <summary>
    /// ���ٵ�����ֵ
    /// </summary>
    public float num;

    public BodyPart(int id, string type, string name, int blood, int weight, float num)
    {
        this.id = id;
        this.type = type;
        this.name = name;
        this.blood = blood;
        this.weight = weight;
        this.num = num;
    }

    
}


public enum FoodHabit
{
    /// <summary>
    /// ��ʳ
    /// </summary>
    herbivore,
    /// <summary>
    /// ��ʳ
    /// </summary>
    Carnivorous,
    /// <summary>
    /// ��ʳ
    /// </summary>
    Omnivorous
}
