grammar Chems;

program: Nl* initial Nl* (rule Nl+)* EOF;

initial: token*;
rule: required '->' provided;
required: token*;
provided: token*;

token: Token;

Token: [a-z];

Nl: [\n\r]+;

WS: [ \t] -> skip;