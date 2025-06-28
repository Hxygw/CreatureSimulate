using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class Animal
{
    /// <summary>
    /// 发生突变的概率(0.00--1.00)
    /// </summary>
    private const float Probability = 0.1f;



    public AnimalType animalType;
    public readonly float speed, attack, range, power, breath, hungrySpeed, acceleration, size, loveSatietyCoast = 0.3f;
    public readonly int blood, weight;
    public readonly Dictionary<int,List<Gene>> genePool = new();
    public readonly List<Attribute> attributes = new();
    public readonly List<Gene> X1chromosomeGenes = new();
    public readonly List<Gene> YorX2chromosomeGenes = new();
    /// <summary>
    /// true为公,false为母
    /// </summary>
    public bool sex;


    public Animal(AnimalType animalType, Dictionary<int,List<Gene>> genePool,bool sex)
    {
        this.animalType = animalType;
        attack = animalType.attack;
        range = animalType.range;
        power = animalType.power;
        breath = animalType.breath;
        loveSatietyCoast = animalType.loveSatietyCoast;
        blood = animalType.blood;
        weight = animalType.weight;
        this.sex = sex;

        if (genePool != null) foreach (int id in genePool.Keys)
        {
            if (genePool[id] == null || genePool[id].Count != 2 || genePool[id][0].geneId != id || id != genePool[id][1].geneId)
                throw new System.Exception("错误的等位基因");
            this.genePool.Add(id, genePool[id]);
            if (genePool[id][0].chromosome != Chromosome.auto || genePool[id][1].chromosome != Chromosome.auto)
            {
                if (genePool[id][0].chromosome == Chromosome.auto || genePool[id][1].chromosome == Chromosome.auto)
                    throw new System.Exception("错误的性染色体基因");
                if (sex)
                {
                    if (genePool[id][0].chromosome == genePool[id][1].chromosome)
                        throw new System.Exception("错误的性染色体基因");
                    foreach (Gene g in genePool[id])
                        if (g.chromosome == Chromosome.x)
                            X1chromosomeGenes.Add(g);
                        else if (g.chromosome == Chromosome.y)
                            YorX2chromosomeGenes.Add(g);
                }
                else
                {
                    if (genePool[id][0].chromosome != Chromosome.x || genePool[id][1].chromosome != Chromosome.x)
                        throw new System.Exception("错误的性染色体基因");
                    X1chromosomeGenes.Add(genePool[id][0]);
                    YorX2chromosomeGenes.Add(genePool[id][1]);
                }
            }
            Gene gene = genePool[id][0].priority > (genePool[id].Count == 2 ? genePool[id][1].priority : -1) ? genePool[id][0] : genePool[id][1];
            foreach (Attribute attribute in gene.attributes)
                switch (attribute.attributeName)
                {
                    case "blood":
                        blood += (int)attribute.num;
                        break;
                    case "range":
                        range += attribute.num;
                        break;
                    case "power":
                        power += attribute.num;
                        break;
                    case "attack":
                        attack += attribute.num;
                        break;
                    case "breath":
                        breath += attribute.num;
                        break;
                    case "loveSatietyCoast":
                        loveSatietyCoast += attribute.num;
                        break;
                    //case "strepsiptera"://捻翅目寄生虫:繁殖后产生大量后代,但亲代立刻死亡
                    //    loveSatietyCoast = StrepsipteraLoveCoast;
                    //    strepsiptera = true;
                    //    break;
                    default:
                        Debug.Log("other attribute");
                        attributes.Add(attribute);
                        break;
                }
        }
        if (blood < 0)
            throw new System.ArgumentException("血液不足以支撑");
        if (breath <= 0)
            throw new System.ArgumentException("供氧不足");
        speed = Mathf.Max(power, 1) / weight;
        hungrySpeed = Mathf.Pow(weight, 1.5f) * (Mathf.Log(power + 2, 2) + 10);
        acceleration = Mathf.Max(power, 1) * Mathf.Pow(blood + 1f, 0.8f) / Mathf.Pow(weight, 0.8f);
        size = Mathf.Pow(weight, 0.8f);
        if (range == 0) range = 1;
    }

    public static Animal CreatNewAnimal(Animal animal1, Animal animal2)
    {
        if(animal1.animalType!=animal2.animalType)
            throw new System.ArgumentException("物种不同");
        if (animal1.sex == animal2.sex)
            throw new System.ArgumentException("同性相交");
        Animal male = animal1, female = animal2;
        if (animal2.sex)
        {
            male = animal2;
            female = animal1;
        }
        bool sex = Random.value >= 0.5f;
        Dictionary<int, List<Gene>> genePool = new();
        foreach (int id in male.genePool.Keys)
        {
            Gene g = male.genePool[id][Random.Range(0, male.genePool[id].Count)];
            if (g.chromosome == Chromosome.auto)
                genePool.Add(id, new List<Gene>() { g, new("", Chromosome.auto, g.geneId, "", -1) });
        }
        foreach (int id in female.genePool.Keys)
        {
            Gene g = female.genePool[id][Random.Range(0, female.genePool[id].Count)];
            if (g.chromosome == Chromosome.auto)
                if (genePool.ContainsKey(id))
                    genePool[id][1] = g;
                else
                    genePool.Add(id, new List<Gene>() { new("", Chromosome.auto, g.geneId, "", -1), g });
        }
        foreach (Gene gene in sex ? male.YorX2chromosomeGenes : male.X1chromosomeGenes)
            genePool.Add(gene.geneId, new() { gene, new("", Chromosome.auto, gene.geneId, "", -1) });
        foreach (Gene gene in Random.value >= 0.5f ? female.X1chromosomeGenes : female.YorX2chromosomeGenes)
            if (genePool.ContainsKey(gene.geneId))
                genePool[gene.geneId][1] = gene;
            else
                genePool.Add(gene.geneId, new List<Gene>() { new("", Chromosome.auto, gene.geneId, "", -1), gene });
        if (Random.value <= Probability)
        {
            Gene gene = Gene.Genes[Random.Range(0,Gene.Genes.Count)];
            if (gene.chromosome == Chromosome.auto)
                if (genePool.ContainsKey(gene.geneId))
                    genePool[gene.geneId][Random.Range(0, 2)] = gene;
                else
                    genePool.Add(gene.geneId, new() { gene, new("", Chromosome.auto, gene.geneId, "", -1) });
            else if (sex)
                if (gene.chromosome == Chromosome.y)
                    if (genePool.ContainsKey(gene.geneId))
                        genePool[gene.geneId][0] = gene;
                    else
                        genePool.Add(gene.geneId, new() { gene, new("", Chromosome.auto, gene.geneId, "", -1) });
                else if (genePool.ContainsKey(gene.geneId))
                    genePool[gene.geneId][1] = gene;
                else
                    genePool.Add(gene.geneId, new() { new("", Chromosome.auto, gene.geneId, "", -1), gene });
            else if(gene.chromosome==Chromosome.x)
                if (genePool.ContainsKey(gene.geneId))
                    genePool[gene.geneId][Random.Range(0, 2)] = gene;
                else
                    genePool.Add(gene.geneId, new() { gene, new("", Chromosome.auto, gene.geneId, "", -1) });
        }
        if (CheckOut(male.animalType, genePool))
            return new Animal(male.animalType, genePool, sex);
        return null;
    }

    /// <summary>
    /// 检查生物能否存活
    /// </summary>
    /// <param name="animalType"></param>
    /// <param name="genes"></param>
    /// <returns></returns>
    public static bool CheckOut(AnimalType animalType, Dictionary<int, List<Gene>> genePool)
    {
        int blood = animalType.blood;
        float breath=animalType.breath;
        if(genePool!=null) foreach (int id in genePool.Keys)
        {
            if (genePool[id] == null || genePool[id].Count == 0 || genePool[id][0].geneId != id || genePool[id].Count == 2 && id != genePool[id][1].geneId)
                throw new System.Exception("错误的等位基因");
            Gene gene = genePool[id][0].priority > (genePool[id].Count == 2 ? genePool[id][1].priority : -1) ? genePool[id][0] : genePool[id][1];
            foreach (Attribute attribute in gene.attributes)
                switch (attribute.attributeName)
                {
                    case "blood":
                        blood += (int)attribute.num;
                        break;
                    case "breath":
                        breath += attribute.num;
                        break;
                }
        }
        return blood >= 0 && breath > 0;
    }
}


/// <summary>
/// 物种
/// </summary>
public class AnimalType
{
    public const float StrepsipteraLoveCoast = 0.2f;
    const string AnimalTypeAddress = "D:\\Unity文件夹\\CreatureSimulate\\Assets\\AnimalDate\\AnimalTypes.txt";
    static string AnimalTypeText;
    /// <summary>
    /// 已经产生的所有生物种类
    /// </summary>
    public static List<AnimalType> AnimalTypes = new();



    public readonly Color color;
    public readonly int id;
    public readonly List<BodyPart> bodyParts = new();
    public readonly float attack, range, power, breath, loveSatietyCoast = 0.3f;
    public readonly int blood, weight;
    public bool strepsiptera = false;
    public FoodHabit foodHabit;
    /// <summary>
    /// 身体部件id列表，用于生物种类的查重
    /// </summary>
    protected readonly List<int> bodyPartIdList = new();

    public AnimalType(FoodHabit foodHabit, List<BodyPart> bodyParts/*,List<Gene> genes*/, int id)
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
                case "Strepsiptera"://捻翅目寄生虫:繁殖后产生大量后代,但亲代立刻死亡
                    loveSatietyCoast = StrepsipteraLoveCoast;
                    strepsiptera = true;
                    break;
                default:
                    throw new System.Exception("错误的部件类型");
            }
            blood -= part.blood;
            weight += part.weight;
            bodyPartIdList.Add(part.id);
        }
        if (blood < 0)
            throw new System.ArgumentException("血液不足以支撑");
        if (breath == 0)
            throw new System.ArgumentException("没有肺");
        bodyPartIdList.Sort();

        color = Random.ColorHSV();
    }

    /// <summary>
    /// 检查物种能否存在
    /// </summary>
    /// <param name="bodyParts"></param>
    /// <returns></returns>
    private static bool CheckOut(List<BodyPart> bodyParts/*,List<Gene> genes*/)
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
    /// 初始化AnimalType
    /// </summary>
    /// <returns></returns>
    public static void Setup()
    {
        try { AnimalTypeText = File.ReadAllText(AnimalTypeAddress); }
        catch { throw new System.Exception("AnimalTypes.txt路径错误(或其他问题)"); }
        string[] animalTypes = AnimalTypeText.Split("\r\n");
        string[][] words = new string[animalTypes.Length][];
        for (int i = 0; i < animalTypes.Length; i++)
            words[i] = animalTypes[i].Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            List<BodyPart> parts = new();
            List<Gene> gene = new();
            for(int j = 2; j < words[i].Length;j++)
                if (BodyPart.BodyPartTable.ContainsKey(words[i][j]))
                    parts.Add(BodyPart.BodyPartTable[words[i][j]]);
                else if (Gene.GeneTable.ContainsKey(words[i][j]))
                    gene.Add(Gene.GeneTable[words[i][j]]);
                else
                {
                    Debug.Log("AnimalTypes.txt第" + (i + 1).ToString() + "行存在错误数据");
                    continue;
                }
            if (CheckOut(parts/*, gene*/) && int.TryParse(words[i][1], out int k))
            {
                if (k < 0 || k >= 3)
                {
                    Debug.Log("AnimalTypes.txt第" + (i + 1).ToString() + "行存在错误数据");
                    continue;
                }
                AnimalType type = new((FoodHabit)k, parts/*, gene*/, AnimalTypes.Count);
                foreach (var t in AnimalTypes)
                    if (ListEquals(t.bodyPartIdList, type.bodyPartIdList))
                    {
                        Debug.Log("AnimalTypes.txt第" + (i + 1).ToString() + "行存在重复数据");
                        continue;
                    }
                AnimalTypes.Add(type);
            }
            else
                Debug.Log("AnimalTypes.txt第" + (i + 1).ToString() + "行存在无法实现的生物类型");
        }
    }

    /*public static AnimalType CreatNewAnimal(AnimalType animalType1,AnimalType animalType2)
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
    }*/


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


public class BodyPart
{
    const string BodyPartAddress = "D:\\Unity文件夹\\CreatureSimulate\\Assets\\AnimalDate\\BodyParts.txt";
    static string BodyPartText;
    public static Dictionary<string, BodyPart> BodyPartTable = new();
    /// <summary>
    /// 所有的身体部件
    /// </summary>
    public static List<BodyPart> BodyParts = new();




    public int id;
    public string type;
    public string name;
    public int blood, weight;
    /// <summary>
    /// 器官的属性值
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

    

    public static void Setup()
    {
        try { BodyPartText = File.ReadAllText(BodyPartAddress); }
        catch { throw new System.Exception("BodyParts.txt路径错误(或其他问题)"); }
        string[] bodyParts = BodyPartText.Split("\r\n");
        string[][] words = new string[bodyParts.Length][];
        for (int i = 0; i < words.Length; i++)
            words[i] = bodyParts[i].Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length != 5)
            {
                Debug.Log("BodyParts.txt第" + (i + 1).ToString() + "行存在错误数据");
                continue;
            }
            if (BodyPartTable.ContainsKey(words[i][1]))
            {
                Debug.Log("BodyParts.txt第" + (i + 1).ToString() + "行数据重复");
                continue;
            }
            if (int.TryParse(words[i][2], out int v1) && int.TryParse(words[i][3], out int v2) && float.TryParse(words[i][4], out float v3))
            {
                BodyParts.Add(new BodyPart(BodyParts.Count, words[i][0], words[i][1], v1, v2, v3));
                BodyPartTable.Add(words[i][1], BodyParts[^1]);
            }
            else
            {
                Debug.Log("BodyParts.txt第" + (i + 1).ToString() + "行数据重复");
                continue;
            }
        }
    }
}

public class Gene
{
    const string GeneAddress = "D:\\Unity文件夹\\CreatureSimulate\\Assets\\AnimalDate\\Gene.txt";
    static string GeneText;
    public static Dictionary<string, Gene> GeneTable = new();
    /// <summary>
    /// 等位基因库
    /// </summary>
    public static Dictionary<int, List<Gene>> AllelePool = new();
    /// <summary>
    /// 所有基因
    /// </summary>
    public static List<Gene> Genes = new();


    public string name;
    public Chromosome chromosome;
    public int geneId;
    public string traitType;
    public int priority;
    public readonly List<Attribute> attributes;

    public Gene(string name,Chromosome chromosome,int geneId,string traitType,int priority)
    {
        this.name = name;
        this.chromosome = chromosome;
        this.geneId = geneId;
        this.traitType = traitType;
        this.priority = priority;
        attributes = new List<Attribute>();
    }


    public static void Setup()
    {
        try { GeneText = File.ReadAllText(GeneAddress); }
        catch { throw new System.Exception("AnimalTypes.txt路径错误(或其他问题)"); }
        string[] genes = GeneText.Split("\r\n");
        string[][] words = new string[genes.Length - 1][];
        for (int i = 0; i < words.Length; i++)
            words[i] = genes[i + 1].Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length < 5)
            {
                Debug.Log("Gene.txt第" + (i + 1).ToString() + "行存在错误数据");
                continue;
            }
            Chromosome chromosome;
            switch (words[i][1])
            {
                case "a":
                    chromosome = Chromosome.auto;
                    break;
                case "x":
                    chromosome = Chromosome.x;
                    break;
                case "y":
                    chromosome = Chromosome.y;
                    break;
                default:
                    Debug.Log(words[i][1]);
                    Debug.Log("Gene.txt第" + (i + 1).ToString() + "行存在错误数据");
                    continue;
            }
            if (int.TryParse(words[i][2], out int v1) && int.TryParse(words[i][4], out int v2))
            {
                Gene gene = new(words[i][0], chromosome, v1, words[i][3], v2);
                for (int j = 5; j < words[i].Length; j += 2)
                    if (float.TryParse(words[i][j + 1], out float value))
                        gene.attributes.Add(new Attribute(words[i][j], value));
                    else
                    {
                        Debug.Log("Gene.txt第" + (i + 1).ToString() + "行存在错误属性");
                        continue;
                    }
                Genes.Add(gene);
                GeneTable.Add(gene.name, gene);
                if (AllelePool.ContainsKey(gene.geneId))
                    AllelePool[gene.geneId].Add(gene);
                else
                    AllelePool.Add(gene.geneId, new() { gene });
            }
            else
            {
                Debug.Log("Gene.txt第" + (i + 1).ToString() + "行存在错误数据");
                continue;
            }
        }
    }
}

public class Attribute
{
    public string attributeName;
    public float num;

    public Attribute(string attributeName, float num)
    {
        this.attributeName = attributeName;
        this.num = num;
    }
}


public enum FoodHabit
{
    /// <summary>
    /// 草食
    /// </summary>
    herbivore,
    /// <summary>
    /// 肉食
    /// </summary>
    Carnivorous,
    /// <summary>
    /// 杂食
    /// </summary>
    Omnivorous
}

public enum Chromosome
{
    /// <summary>
    /// 常染色体
    /// </summary>
    auto,
    /// <summary>
    /// x染色体
    /// </summary>
    x,
    /// <summary>
    /// y染色体
    /// </summary>
    y
}
