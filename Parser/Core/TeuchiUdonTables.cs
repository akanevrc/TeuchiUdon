using System.Collections.Generic;

namespace akanevrc.TeuchiUdon
{
    public class TeuchiUdonTables
    {
        public List<TeuchiUdonParserResult> ParserResults { get; private set; }
        public List<TeuchiUdonParserError> ParserErrors { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonVar> Vars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonLiteral> PublicVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonSyncMode> SyncedVars { get; private set; }
        public Dictionary<TeuchiUdonVar, TeuchiUdonFunc> EventFuncs { get; private set; }
        public Dictionary<TeuchiUdonLiteral, TeuchiUdonLiteral> Literals { get; private set; }
        public Dictionary<TeuchiUdonThis, TeuchiUdonThis> This { get; private set; }
        public Dictionary<TeuchiUdonFunc, TeuchiUdonFunc> Funcs { get; private set; }
        public Dictionary<TeuchiUdonIndirect, uint> Indirects { get; private set; }
        public Dictionary<TeuchiUdonBlock, TeuchiUdonBlock> Blocks { get; private set; }
        public HashSet<IDataLabel> UsedData { get; set; }

        private int VarCounter { get; set; }
        private int OutValueCounter { get; set; }
        private int LiteralCounter { get; set; }
        private int FuncCounter { get; set; }
        private int VarBindCounter { get; set; }
        private int BlockCounter { get; set; }
        private int EvalFuncCounter { get; set; }
        private int LetInCounter { get; set; }
        private int BranchCounter { get; set; }
        private int LoopCounter { get; set; }
        private int IndirectCounter { get; set; }

        private TeuchiUdonPrimitives Primitives { get; }

        public TeuchiUdonTables(TeuchiUdonPrimitives primitives)
        {
            Primitives = primitives;

            ParserResults = new List<TeuchiUdonParserResult>();
            ParserErrors  = new List<TeuchiUdonParserError>();
            Vars          = new Dictionary<TeuchiUdonVar     , TeuchiUdonVar>();
            PublicVars    = new Dictionary<TeuchiUdonVar     , TeuchiUdonLiteral>();
            SyncedVars    = new Dictionary<TeuchiUdonVar     , TeuchiUdonSyncMode>();
            EventFuncs    = new Dictionary<TeuchiUdonVar     , TeuchiUdonFunc>();
            Literals      = new Dictionary<TeuchiUdonLiteral , TeuchiUdonLiteral>();
            This          = new Dictionary<TeuchiUdonThis    , TeuchiUdonThis>();
            Funcs         = new Dictionary<TeuchiUdonFunc    , TeuchiUdonFunc>();
            Indirects     = new Dictionary<TeuchiUdonIndirect, uint>();
            Blocks        = new Dictionary<TeuchiUdonBlock   , TeuchiUdonBlock>();
            UsedData      = new HashSet<IDataLabel>();

            VarCounter      = 0;
            OutValueCounter = 0;
            LiteralCounter  = 0;
            FuncCounter     = 0;
            VarBindCounter  = 0;
            BlockCounter    = 0;
            EvalFuncCounter = 0;
            LetInCounter    = 0;
            BranchCounter   = 0;
            LoopCounter     = 0;
            IndirectCounter = 0;
        }

        public int GetVarIndex()
        {
            return VarCounter++;
        }

        public int GetLiteralIndex()
        {
            return LiteralCounter++;
        }

        public int GetFuncIndex()
        {
            return FuncCounter++;
        }

        public int GetVarBindIndex()
        {
            return VarBindCounter++;
        }

        public int GetBlockIndex()
        {
            return BlockCounter++;
        }

        public int GetEvalFuncIndex()
        {
            return EvalFuncCounter++;
        }

        public int GetLetInIndex()
        {
            return LetInCounter++;
        }

        public int GetBranchIndex()
        {
            return BranchCounter++;
        }

        public int GetLoopIndex()
        {
            return LoopCounter++;
        }

        public int GetIndirectIndex()
        {
            return IndirectCounter++;
        }
    }
}
