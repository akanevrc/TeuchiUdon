//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from .\TeuchiUdonParser.g4 by ANTLR 4.9.3

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace akanevrc.TeuchiUdon.Editor {

    #pragma warning disable 3021

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="TeuchiUdonParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.3")]
[System.CLSCompliant(false)]
public interface ITeuchiUdonParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.target"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTarget([NotNull] TeuchiUdonParser.TargetContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.target"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTarget([NotNull] TeuchiUdonParser.TargetContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBody([NotNull] TeuchiUdonParser.BodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.body"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBody([NotNull] TeuchiUdonParser.BodyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.topBind"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTopBind([NotNull] TeuchiUdonParser.TopBindContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.topBind"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTopBind([NotNull] TeuchiUdonParser.TopBindContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.varBind"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVarBind([NotNull] TeuchiUdonParser.VarBindContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.varBind"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVarBind([NotNull] TeuchiUdonParser.VarBindContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>TupleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTupleVarDecl([NotNull] TeuchiUdonParser.TupleVarDeclContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>TupleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTupleVarDecl([NotNull] TeuchiUdonParser.TupleVarDeclContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>SingleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSingleVarDecl([NotNull] TeuchiUdonParser.SingleVarDeclContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>SingleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSingleVarDecl([NotNull] TeuchiUdonParser.SingleVarDeclContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.qualified"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterQualified([NotNull] TeuchiUdonParser.QualifiedContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.qualified"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitQualified([NotNull] TeuchiUdonParser.QualifiedContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIdentifier([NotNull] TeuchiUdonParser.IdentifierContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIdentifier([NotNull] TeuchiUdonParser.IdentifierContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalVarExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEvalVarExpr([NotNull] TeuchiUdonParser.EvalVarExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalVarExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEvalVarExpr([NotNull] TeuchiUdonParser.EvalVarExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AccessExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAccessExpr([NotNull] TeuchiUdonParser.AccessExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AccessExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAccessExpr([NotNull] TeuchiUdonParser.AccessExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalUnitFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEvalUnitFuncExpr([NotNull] TeuchiUdonParser.EvalUnitFuncExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalUnitFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEvalUnitFuncExpr([NotNull] TeuchiUdonParser.EvalUnitFuncExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalTupleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEvalTupleFuncExpr([NotNull] TeuchiUdonParser.EvalTupleFuncExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalTupleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEvalTupleFuncExpr([NotNull] TeuchiUdonParser.EvalTupleFuncExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ParensExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParensExpr([NotNull] TeuchiUdonParser.ParensExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ParensExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParensExpr([NotNull] TeuchiUdonParser.ParensExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>LiteralExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLiteralExpr([NotNull] TeuchiUdonParser.LiteralExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>LiteralExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLiteralExpr([NotNull] TeuchiUdonParser.LiteralExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>FuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncExpr([NotNull] TeuchiUdonParser.FuncExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>FuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncExpr([NotNull] TeuchiUdonParser.FuncExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalSingleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEvalSingleFuncExpr([NotNull] TeuchiUdonParser.EvalSingleFuncExprContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalSingleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEvalSingleFuncExpr([NotNull] TeuchiUdonParser.EvalSingleFuncExprContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>IntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIntegerLiteral([NotNull] TeuchiUdonParser.IntegerLiteralContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>IntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIntegerLiteral([NotNull] TeuchiUdonParser.IntegerLiteralContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>HexIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHexIntegerLiteral([NotNull] TeuchiUdonParser.HexIntegerLiteralContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>HexIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHexIntegerLiteral([NotNull] TeuchiUdonParser.HexIntegerLiteralContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>BinIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBinIntegerLiteral([NotNull] TeuchiUdonParser.BinIntegerLiteralContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>BinIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBinIntegerLiteral([NotNull] TeuchiUdonParser.BinIntegerLiteralContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RealLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRealLiteral([NotNull] TeuchiUdonParser.RealLiteralContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RealLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRealLiteral([NotNull] TeuchiUdonParser.RealLiteralContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>CharacterLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCharacterLiteral([NotNull] TeuchiUdonParser.CharacterLiteralContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>CharacterLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCharacterLiteral([NotNull] TeuchiUdonParser.CharacterLiteralContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>RegularString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRegularString([NotNull] TeuchiUdonParser.RegularStringContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>RegularString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRegularString([NotNull] TeuchiUdonParser.RegularStringContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>VervatiumString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVervatiumString([NotNull] TeuchiUdonParser.VervatiumStringContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>VervatiumString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVervatiumString([NotNull] TeuchiUdonParser.VervatiumStringContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.open"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterOpen([NotNull] TeuchiUdonParser.OpenContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.open"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitOpen([NotNull] TeuchiUdonParser.OpenContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.close"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClose([NotNull] TeuchiUdonParser.CloseContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.close"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClose([NotNull] TeuchiUdonParser.CloseContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.end"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEnd([NotNull] TeuchiUdonParser.EndContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.end"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEnd([NotNull] TeuchiUdonParser.EndContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.newline"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNewline([NotNull] TeuchiUdonParser.NewlineContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.newline"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNewline([NotNull] TeuchiUdonParser.NewlineContext context);
}
} // namespace akanevrc.TeuchiUdon.Editor
