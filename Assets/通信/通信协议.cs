using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ս�׸���.�����ռ�;
using static TGZG.�����ռ�;
using TGZG;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace ս�׸��� {
    //�����б�ͨѶЭ��
    public struct ���з��������� {
        public List<����������> �����б�;
        [JsonIgnore]
        public int �������� => �����б�.Count;
        [JsonIgnore]
        public ���������� this[int index] => �����б�[index];
        [JsonIgnore]
        public ���������� this[string ������] => �����б�.Find(x => x.������ == ������);
        public bool ���ڷ���(string ������) => �����б�.Exists(x => x.������ == ������);
    }
    public struct ���������� {
        public string IP;
        public string ������;
        public string ��������;
        public string ����;
        public int ����;
        public string ��ͼ��;
        public bool ��������;
        public string ����汾;
        public int ÿ��ͬ������;
        public DateTime ���䴴��ʱ��;
    }
    //��ϷͨѶ��ÿ�뷢����ʮ�Ρ�Ϊ�˼��ʹ���ѹ�����ֶ�����������Ҫ���̡ܶ�
    public struct ����������� {
        public ��ҵ�¼���� u;
        public ����������� p;
        public ���� tm;
        public int[] ��;
        public List<������������> msl;
        public void ��λ����() {
            //����floatֻ������λС��
            p.p = p.p.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.d = p.d.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.v = p.v.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.r = p.r.Select(t => (float)Math.Round(t, 3)).ToArray();
            for (int i = 0; i < msl.Count; i++) {
                var n = new ������������();
                n.i = msl[i].i;
                n.tp = msl[i].tp;
                n.p = msl[i].p.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.d = msl[i].d.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.v = msl[i].v.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.r = msl[i].r.Select(t => (float)Math.Round(t, 3)).ToArray();
                msl[i] = n;
            }
        }
    }
    public struct ������������ {
        public int i;
        public �������� tp;
        public float[] p;
        public float[] d;
        public float[] v;
        public float[] r;
    }
    public enum �������� {
        AIM9E,
    }
    public enum ��λ {
        ��,
        ��,
        ����,
        ����,
        ����,
        ����,
        ��β,
        ��β,
        ��,
    }
    public struct ������Ϣ {
        public string ths;
        public float dm;
        public ��λ bp;
    }
    public struct ��ҵ�¼���� {
        public string n;
        public �ؾ����� tp;
    }
    public struct ����������� {
        public float[] p;
        public float[] d;
        public float[] v;
        public float[] r;
    }
    public enum �ؾ����� {
        ��,
        m15n23,
        f86f25,
        f4c,
        m21pfm,
        P51h
    }
    public enum ���� {
        ��,
        ��,
        ��,
        ϵͳ
    }
}
